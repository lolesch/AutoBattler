using System.Collections;
using System.Linq;
using System.Text;
using Code.Data.Enums;
using Code.Data.Items.Activator;
using Code.Runtime.Inventory;
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

        private float       _anchoredX;
        private Coroutine   _pendingShow;
        private ITetrisItem _pendingItem;
        private ITetrisItem _visibleItem;
        private ITetrisItem _pendingHideItem;
        private bool        _hideScheduled;

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

            if (_panel.gameObject.activeSelf)
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

            _pendingShow = null;
            _pendingItem = null;
            _visibleItem = item;

            var topology = ChainResolver.ResolveTopology(container);
            _text.text   = BuildTooltip(item, topology);

            var isRoot = topology.Roots.Contains(item);
            _panelFrame.color = ChainComponentColors.GetColor(item, isRoot);

            _panel.pivot = new Vector2(onRight ? 1f : 0f, 1f);
            _panel.gameObject.SetActive(true);
            LayoutRebuilder.ForceRebuildLayoutImmediate(_panel);

            _anchoredX           = CanvasX(anchorScreenX);
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
            _visibleItem = null;
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

        private static string BuildTooltip(ITetrisItem item, ChainTopology topology)
        {
            var sb = new StringBuilder();

            sb.AppendLine($"<b>[{ComponentLabel(item)}]</b>  {item.Name}");
            sb.AppendLine(new string('─', 24));
            AppendItemStats(sb, item);

            var chain = topology.Chains.FirstOrDefault(
                c => c.Root == item || c.Modifiers.Contains(item));

            if (chain is not { IsValid: true })
            {
                sb.AppendLine("  (not connected)");
                return sb.ToString().TrimEnd();
            }

            if (item is IWeaponItem && item != chain.Root)
                sb.AppendLine($"  payload condition: {((IWeaponItem)item).PayloadCondition}");

            sb.AppendLine();
            sb.AppendLine(BuildChainSentence(chain, item));
            AppendChainOutput(sb, chain);

            return sb.ToString().TrimEnd();
        }

        private static void AppendItemStats(StringBuilder sb, ITetrisItem item)
        {
            switch (item)
            {
                case IWeaponItem w:
                    sb.AppendLine($"  dmg  {(float)w.Damage:F1}   spd  {(float)w.AttackSpeed:F1}");
                    sb.AppendLine($"  cost {(float)w.ResourceCost:F1}   gen  {(float)w.ResourceGenOnHit:F1}");
                    if (w.PayloadCondition != PayloadConditionType.None)
                    {
                        sb.AppendLine();
                        sb.AppendLine($"  payload: {w.PayloadCondition}  ×{w.PayloadDamageMultiplier:F2}");
                        sb.AppendLine($"  threshold: {w.PayloadConditionThreshold:F2}");
                    }
                    break;

                case IAmplifierItem amp:
                    var weaponMod = amp.WeaponModifier;
                    sb.AppendLine($"  chain:    {weaponMod.AttackStat} {weaponMod.Modifier}");
                    if (amp is IStatModifier statMod)
                        foreach (var affix in statMod.Affixes)
                            sb.AppendLine($"  passive:  {affix.stat} {affix.modifier}");
                    break;

                case IActivatorItem act:
                    sb.AppendLine($"  modifies: {act.WeaponStat} {FormatModifier(act.Value, act.ModifierType)}");
                    sb.AppendLine($"  when:     {act.ConditionType}");
                    if (act.ConditionType != ActivatorConditionType.Always)
                        sb.AppendLine($"  threshold:{act.ConditionThreshold:F2}");
                    break;

                case IReactorItem reactor:
                    sb.AppendLine($"  trigger:  {reactor.ReactorType}");
                    break;

                case IConverterItem:
                    sb.AppendLine("  (converter — not yet implemented)");
                    break;
            }
        }

        private static string FormatModifier(float value, ModifierType type) => type switch
        {
            ModifierType.Overwrite   => $"= {value:0.###}",
            ModifierType.FlatAdd     => $"{value:+0.###;0.###;-0.###}",
            ModifierType.PercentAdd  => $"{value:+0.###;0.###;-0.###} %",
            ModifierType.PercentMult => $"* {value:0.###} %",
            _                        => $"{value:0.###}",
        };

        private static void AppendChainOutput(StringBuilder sb, IItemChain chain)
        {
            var weapon = chain.Weapon;
            if (weapon == null) return;

            chain.ApplyChainModifiers();

            sb.AppendLine();
            sb.AppendLine("<b>Chain output:</b>");
            sb.AppendLine($"  dmg  {(float)weapon.Damage:F1}   spd  {(float)weapon.AttackSpeed:F1}");
            sb.AppendLine($"  cost {(float)weapon.ResourceCost:F1}");

            chain.RemoveChainModifiers();
        }

        private static string BuildChainSentence(IItemChain chain, ITetrisItem hovered)
        {
            var sb = new StringBuilder("<b>Chain:</b>  ");

            void Append(ITetrisItem item, bool isRoot)
            {
                var label = item is IWeaponItem && !isRoot ? "Payload" : ComponentLabel(item);
                var name  = item == hovered ? $"<b>[{item.Name}]</b>" : item.Name;
                sb.Append($"{label}({name})");
            }

            Append(chain.Root, true);
            foreach (var mod in chain.Modifiers) { sb.Append(" → "); Append(mod, false); }

            return sb.ToString();
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
    }

    public interface IItemTooltipController
    {
        void RequestShow(ITetrisItem item, ITetrisContainer container, float anchorScreenX, bool onRight);
        void Hide(ITetrisItem leavingItem);
    }
}