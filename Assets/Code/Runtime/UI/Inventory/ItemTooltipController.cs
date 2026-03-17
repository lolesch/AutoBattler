using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Code.Data.Enums;
using Code.Data.Items.Activator;
using Code.Runtime.Inventory;
using Code.Runtime.Statistics;
using Submodules.Utility.Extensions;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Code.Runtime.UI.Inventory
{
    [RequireComponent(typeof(Canvas))]
    public sealed class ItemTooltipController : MonoBehaviour, IItemTooltipController
    {
        [SerializeField] private RectTransform _panel;
        [SerializeField] private Image         _panelFrame;
        [SerializeField] private TMP_Text      _text;
        [SerializeField] private Canvas        _canvas;
        [SerializeField] private float         _showDelay = 0.4f;

        private float         _anchoredX;
        private Coroutine     _pendingShow;
        private ITetrisItem   _pendingItem;
        private ITetrisItem   _visibleItem;
        private ITetrisItem   _pendingHideItem;
        private bool          _hideScheduled;
        private ITetrisItem   _cachedItem;
        private ChainTopology _cachedTopology;
        private bool          _altWasPressed;

        private static readonly Color LightGray = new(0.7f, 0.7f, 0.7f);

        private void Awake()
        {
            if (_canvas == null)
            {
                _canvas = GetComponent<Canvas>();
                Debug.LogWarning("Assign _canvas in Inspector.", this);
            }
            _panel.gameObject.SetActive(false);
        }

        private void LateUpdate()
        {
            if (_hideScheduled)
            {
                _hideScheduled = false;
                ExecuteHide(_pendingHideItem);
                _pendingHideItem = null;
            }

            if (!_panel.gameObject.activeSelf) return;

            var altNow = Input.GetKey(KeyCode.LeftAlt) || Input.GetKey(KeyCode.RightAlt);
            if (altNow != _altWasPressed)
            {
                _altWasPressed = altNow;
                if (_cachedItem != null && _cachedTopology != null)
                {
                    _text.text = BuildTooltip(_cachedItem, _cachedTopology, altNow);
                    LayoutRebuilder.ForceRebuildLayoutImmediate(_panel);
                }
            }

            _panel.anchoredPosition = ClampedPosition();
        }

        // ── IItemTooltipController ────────────────────────────────────────

        public void RequestShow(ITetrisItem item, ITetrisContainer container, float anchorScreenX, bool onRight)
        {
            if (item == null) { Hide(null); return; }

            _hideScheduled   = false;
            _pendingHideItem = null;

            if (item == _visibleItem || item == _pendingItem) return;

            if (_pendingShow != null) StopCoroutine(_pendingShow);
            _pendingItem = item;
            _pendingShow = StartCoroutine(ShowAfterDelay(item, container, anchorScreenX, onRight));
        }

        public void Hide(ITetrisItem leavingItem)
        {
            if (leavingItem != null && leavingItem != _visibleItem && leavingItem != _pendingItem) return;

            _pendingHideItem = leavingItem;
            _hideScheduled   = true;
        }

        // ── Internals ─────────────────────────────────────────────────────

        private IEnumerator ShowAfterDelay(ITetrisItem item, ITetrisContainer container,
            float anchorScreenX, bool onRight)
        {
            if (_visibleItem == null)
                yield return new WaitForSeconds(_showDelay);

            _pendingShow  = null;
            _pendingItem  = null;
            _visibleItem  = item;

            var topology    = ChainResolver.ResolveTopology(container);
            _cachedItem     = item;
            _cachedTopology = topology;
            _altWasPressed  = Input.GetKey(KeyCode.LeftAlt) || Input.GetKey(KeyCode.RightAlt);
            _text.text      = BuildTooltip(item, topology, _altWasPressed);

            var primaryChain  = PrimaryChain(item, topology);
            var isWeaponRoot  = item is IWeaponItem && primaryChain != null && !IsPayload(item, primaryChain);
            _panelFrame.color = ChainComponentColors.GetColor(item, isWeaponRoot);

            _panel.pivot = new Vector2(onRight ? 1f : 0f, 1f);
            _panel.gameObject.SetActive(true);
            LayoutRebuilder.ForceRebuildLayoutImmediate(_panel);

            _anchoredX              = CanvasX(anchorScreenX);
            _panel.anchoredPosition = ClampedPosition();
        }

        private void ExecuteHide(ITetrisItem leavingItem)
        {
            if (leavingItem != null && leavingItem != _visibleItem && leavingItem != _pendingItem) return;

            if (_pendingShow != null)
            {
                StopCoroutine(_pendingShow);
                _pendingShow = null;
                _pendingItem = null;
            }
            _visibleItem    = null;
            _cachedItem     = null;
            _cachedTopology = null;
            _panel.gameObject.SetActive(false);
        }

        // ── Position helpers ──────────────────────────────────────────────

        private Vector2 ClampedPosition()
        {
            var canvasRT = (RectTransform)_canvas.transform;
            RectTransformUtility.ScreenPointToLocalPointInRectangle(
                canvasRT, Input.mousePosition, null, out var mouse);

            var half      = canvasRT.rect.size * 0.5f;
            var panelSize = _panel.rect.size;
            var clampedY  = Mathf.Clamp(mouse.y, Mathf.Min(-half.y + panelSize.y, half.y), half.y);

            return new Vector2(_anchoredX, clampedY);
        }

        private float CanvasX(float screenX)
        {
            var canvasRT = (RectTransform)_canvas.transform;
            RectTransformUtility.ScreenPointToLocalPointInRectangle(
                canvasRT, new Vector2(screenX, 0f), null, out var local);

            return Mathf.Clamp(local.x, -canvasRT.rect.width * 0.5f, canvasRT.rect.width * 0.5f);
        }

        // ── Tooltip text ──────────────────────────────────────────────────

        private static string BuildTooltip(ITetrisItem item, ChainTopology topology, bool detailed)
        {
            var sb = new StringBuilder();

            var itemColor    = ChainComponentColors.GetColor(item, item is IWeaponItem);
            var componentStr = $"[{ComponentLabel(item)}]".Colored(itemColor);
            sb.AppendLine($"<align=left><b>{item.Name}</b><align=right> {componentStr}</align>");
            sb.AppendLine(new string('─', 24));

            var chains    = topology.Chains
                .Where(c => c.Root == item || c.Modifiers.Contains(item))
                .ToList();
            var isChained = chains.Count > 0;

            AppendItemStats(sb, item, isChained);

            if (!isChained)
            {
                //sb.AppendLine("  (not connected)");
                return sb.ToString().TrimEnd();
            }

            foreach (var chain in chains)
            {
                sb.AppendLine();
                sb.AppendLine(new string('─', 24));
                if (detailed)
                {
                    sb.Append(BuildChainSentence(chain, item));
                    sb.AppendLine();
                }
                AppendChainOutput(sb, chain, item, detailed);
            }

            return sb.ToString().TrimEnd();
        }

        private static string BuildChainSentence(IItemChain chain, ITetrisItem hovered)
        {
            var ordered  = OrderedItems(chain);
            var hovIndex = ordered.IndexOf(hovered);
            var sb       = new StringBuilder();
            var isFirst  = true;

            void AppendEntry(ITetrisItem item)
            {
                var isPayload    = IsPayload(item, chain);
                var isWeaponRoot = item is IWeaponItem && !isPayload;
                var color        = ChainComponentColors.GetColor(item, isWeaponRoot);
                var name         = item == hovered ? $"<b>{item.Name}</b>" : item.Name;
                var prefix       = isFirst ? "" : "  ↓\n";
                sb.Append($"{prefix}{name.Colored(color)}\n");
                isFirst = false;
            }

            var isTrigger = hovered is IActivatorItem or IReactorItem;
            if (isTrigger)
            {
                AppendEntry(hovered);
                for (var i = hovIndex + 1; i < ordered.Count; i++)
                {
                    AppendEntry(ordered[i]);
                    if (ordered[i] is IWeaponItem) break;
                }
            }
            else
            {
                AppendEntry(hovered);
                for (var i = hovIndex - 1; i >= 0; i--)
                    AppendEntry(ordered[i]);
            }

            return sb.ToString().TrimEnd();
        }

        private static void AppendChainOutput(StringBuilder sb, IItemChain chain,
            ITetrisItem hovered, bool detailed)
        {
            var weapon = chain.Weapon;
            if (weapon == null) return;

            var ordered  = OrderedItems(chain);
            var hovIndex = ordered.IndexOf(hovered);

            var ampsBefore       = ordered.Take(hovIndex).OfType<IAmplifierItem>().ToList();
            var activatorsBefore = ordered.Take(hovIndex).OfType<IActivatorItem>().ToList();
            var hoveredAmp       = hovered as IAmplifierItem;
            var hoveredActivator = hovered as IActivatorItem;

            foreach (var amp in ampsBefore)       ApplyAmp(amp, weapon);
            foreach (var act in activatorsBefore) ApplyActivator(act, weapon);

            float dmgBase  = weapon.Damage;
            float spdBase  = weapon.AttackSpeed;
            float costBase = weapon.ResourceCost;
            float genBase  = weapon.ResourceGenOnHit;

            if (hoveredAmp       != null) ApplyAmp(hoveredAmp, weapon);
            if (hoveredActivator != null) ApplyActivator(hoveredActivator, weapon);

            float dmgFinal  = weapon.Damage;
            float spdFinal  = weapon.AttackSpeed;
            float costFinal = weapon.ResourceCost;
            float genFinal  = weapon.ResourceGenOnHit;

            sb.AppendLine("<b>Output:</b>");
            sb.AppendLine($"  dmg  {Stat(dmgBase,  dmgFinal,  detailed)}   " +
                          $"spd  {Stat(spdBase,  spdFinal,  detailed)}");
            sb.AppendLine($"  cost {Stat(costBase, costFinal, detailed, invert: true)}   " +
                          $"gen  {Stat(genBase,  genFinal,  detailed)}");

            foreach (var amp in ampsBefore)       RemoveAmp(amp, weapon);
            foreach (var act in activatorsBefore) RemoveActivator(act, weapon);
            if (hoveredAmp       != null) RemoveAmp(hoveredAmp, weapon);
            if (hoveredActivator != null) RemoveActivator(hoveredActivator, weapon);
        }

        // ── Modifier apply/remove ─────────────────────────────────────────

        private static void ApplyAmp(IAmplifierItem amp, IWeaponItem weapon)
        {
            var mod = amp.WeaponModifier;
            switch (mod.AttackStat)
            {
                case AttackStatType.Damage:           weapon.Damage.AddModifier(mod.Modifier);           break;
                case AttackStatType.ResourceGenOnHit: weapon.ResourceGenOnHit.AddModifier(mod.Modifier); break;
            }
        }

        private static void RemoveAmp(IAmplifierItem amp, IWeaponItem weapon)
        {
            var mod = amp.WeaponModifier;
            switch (mod.AttackStat)
            {
                case AttackStatType.Damage:           weapon.Damage.TryRemoveAllModifiersBySource(amp.Guid);           break;
                case AttackStatType.ResourceGenOnHit: weapon.ResourceGenOnHit.TryRemoveAllModifiersBySource(amp.Guid); break;
            }
        }

        private static void ApplyActivator(IActivatorItem act, IWeaponItem weapon)
        {
            var mod = new Modifier(act.Value, act.ModifierType, act.Guid);
            switch (act.WeaponStat)
            {
                case FiringStatType.AttackSpeed:  weapon.AttackSpeed.AddModifier(mod);  break;
                case FiringStatType.ResourceCost: weapon.ResourceCost.AddModifier(mod); break;
            }
        }

        private static void RemoveActivator(IActivatorItem act, IWeaponItem weapon)
        {
            switch (act.WeaponStat)
            {
                case FiringStatType.AttackSpeed:  weapon.AttackSpeed.TryRemoveAllModifiersBySource(act.Guid);  break;
                case FiringStatType.ResourceCost: weapon.ResourceCost.TryRemoveAllModifiersBySource(act.Guid); break;
            }
        }

        // ── Stat formatting ───────────────────────────────────────────────

        private static string Stat(float before, float after, bool detailed, bool invert = false)
        {
            if (Mathf.Approximately(before, after))
                return $"{after:F1}";

            var improved  = invert ? after < before : after > before;
            var color     = improved ? new Color(0f, 1f, 0.53f) : new Color(1f, 0.27f, 0.27f);
            var resultStr = $"{after:F1}".Colored(color);

            return detailed ? $"{before:F1} → {resultStr}" : resultStr;
        }

        // ── Item stats display ────────────────────────────────────────────

        private static void AppendItemStats(StringBuilder sb, ITetrisItem item, bool isChained)
        {
            switch (item)
            {
                case IWeaponItem w:
                    sb.AppendLine($"  dmg  {(float)w.Damage:F1}   spd  {(float)w.AttackSpeed:F1}");
                    sb.AppendLine($"  cost {(float)w.ResourceCost:F1}   gen  {(float)w.ResourceGenOnHit:F1}");
                    if (w.PayloadCondition != PayloadConditionType.None)
                    {
                        sb.AppendLine();
                        sb.AppendLine($"  payload:   {w.PayloadCondition}  ×{w.PayloadDamageMultiplier:F2}");
                        sb.AppendLine($"  threshold: {w.PayloadConditionThreshold:F2}");
                    }
                    break;

                case IAmplifierItem amp:
                {
                    var weaponMod    = amp.WeaponModifier;
                    var chainedStr   = $"chained:   {weaponMod.AttackStat} {weaponMod.Modifier}";
                    var hasAffix     = amp is IStatModifier sm && sm.Affixes.Count > 0;
                    var unchainedStr = hasAffix
                        ? $"unchained: {((IStatModifier)amp).Affixes[0].stat} {((IStatModifier)amp).Affixes[0].modifier}"
                        : null;

                    if (isChained)
                    {
                        sb.AppendLine($"  <b>{chainedStr}</b>");
                        if (unchainedStr != null)
                            sb.AppendLine($"  {unchainedStr.Colored(LightGray)}");
                    }
                    else
                    {
                        sb.AppendLine($"  {chainedStr.Colored(LightGray)}");
                        if (unchainedStr != null)
                            sb.AppendLine($"  <b>{unchainedStr}</b>");
                    }
                    break;
                }

                case IActivatorItem act:
                    sb.AppendLine($"  <b>{act.WeaponStat} {FormatModifier(act.Value, act.ModifierType)}</b>");
                    sb.AppendLine($"  when: {(act.ConditionType == ActivatorConditionType.Always ? "always" : $"{act.ConditionType} {act.ConditionThreshold:F2}")}");
                    break;

                case IReactorItem reactor:
                    sb.AppendLine($"  <b>{reactor.ReactorType}</b>");
                    break;

                case IConverterItem:
                    sb.AppendLine("  (converter — not yet implemented)");
                    break;
            }
        }

        // ── Helpers ───────────────────────────────────────────────────────

        private static List<ITetrisItem> OrderedItems(IItemChain chain)
        {
            var list = new List<ITetrisItem> { chain.Root };
            list.AddRange(chain.Modifiers);
            return list;
        }

        private static IItemChain PrimaryChain(ITetrisItem item, ChainTopology topology) =>
            topology.Chains.FirstOrDefault(
                c => (c.Root == item || c.Modifiers.Contains(item)) && !IsPayload(item, c))
            ?? topology.Chains.FirstOrDefault(c => c.Root == item || c.Modifiers.Contains(item));

        private static bool IsPayload(ITetrisItem item, IItemChain chain)
        {
            if (item is not IWeaponItem) return false;
            foreach (var ordered in OrderedItems(chain))
            {
                if (ordered == item)        return false; // no weapon found before self
                if (ordered is IWeaponItem) return true;  // a weapon precedes self
            }
            return false;
        }

        private static string ComponentLabel(ITetrisItem item) => item switch
        {
            IWeaponItem    => "Weapon",
            IAmplifierItem => "Amplifier",
            IConverterItem => "Converter",
            IActivatorItem => "Activator",
            IReactorItem   => "Reactor",
            _              => item.GetType().Name,
        };

        private static string FormatModifier(float value, ModifierType type) => type switch
        {
            ModifierType.Overwrite   => $"= {value:0.###}",
            ModifierType.FlatAdd     => $"{value:+0.###;0.###;-0.###}",
            ModifierType.PercentAdd  => $"{value:+0.###;0.###;-0.###} %",
            ModifierType.PercentMult => $"* {value:0.###} %",
            _                        => $"{value:0.###}",
        };
    }

    public interface IItemTooltipController
    {
        void RequestShow(ITetrisItem item, ITetrisContainer container, float anchorScreenX, bool onRight);
        void Hide(ITetrisItem leavingItem);
    }
}