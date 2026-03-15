using System.Linq;
using System.Text;
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
        [SerializeField] private TMP_Text      _text;
        [SerializeField] private Canvas        _canvas;
        [SerializeField] private Vector2       _cursorOffset = new(16f, -16f);

        private void Awake()
        {
            if (_canvas == null)
            {
                _canvas = GetComponent<Canvas>();
                Debug.LogWarning("Assign _canvas in Inspector.", this);
            }
            _panel.gameObject.SetActive(false);
        }

        private void Update()
        {
            if (!_panel.gameObject.activeSelf) return;

            var canvasRT = (RectTransform)_canvas.transform;
            RectTransformUtility.ScreenPointToLocalPointInRectangle(
                canvasRT, Input.mousePosition, null, out var local);

            var pos       = local + _cursorOffset;
            var half      = canvasRT.rect.size * 0.5f;

            var onRightHalf = local.x > 0f;
            _panel.pivot = new Vector2(onRightHalf ? 1f : 0f, 1f);
            // Flip offset direction so the panel always opens away from the cursor.
            var offsetX = onRightHalf ? -_cursorOffset.x : _cursorOffset.x;
            pos = local + new Vector2(offsetX, _cursorOffset.y);

            var panelSize = _panel.rect.size;
            pos.x = Mathf.Clamp(pos.x, -half.x, half.x);
            pos.y = Mathf.Clamp(pos.y, -half.y + panelSize.y, half.y);

            _panel.anchoredPosition = pos;
        }

        public void Show(ITetrisItem item, ITetrisContainer container)
        {
            if (item == null) { Hide(); return; }
            _text.text = BuildTooltip(item, ChainResolver.ResolveTopology(container));
            _panel.gameObject.SetActive(true);
            LayoutRebuilder.ForceRebuildLayoutImmediate(_panel);
        }

        public void Hide() => _panel.gameObject.SetActive(false);

        // ── Formatting ────────────────────────────────────────────────────

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

            var isPayload = item is IWeaponItem && item != chain.Root;
            if (isPayload)
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
                    sb.AppendLine($"  dmg  {(float)w.Damage:F1}   " +
                                  $"spd  {(float)w.AttackSpeed:F1}");
                    sb.AppendLine($"  cost {(float)w.ResourceCost:F1}   " +
                                  $"gen  {(float)w.ResourceGenOnHit:F1}");
                    break;
                case IAmplifierItem amp:
                    var mod = amp.WeaponModifier;
                    sb.AppendLine($"  {mod.AttackStat}: {mod.Modifier}");
                    break;
                default:
                    sb.AppendLine("  (stats not yet exposed)");
                    break;
            }
        }

        private static void AppendChainOutput(StringBuilder sb, IItemChain chain)
        {
            var weapon = chain.Weapon;
            if (weapon == null) return;

            sb.AppendLine();
            sb.AppendLine("<b>Chain output:</b>");
            sb.AppendLine($"  dmg  {(float)weapon.Damage:F1}   " +
                          $"spd  {(float)weapon.AttackSpeed:F1}");
            sb.AppendLine($"  cost {(float)weapon.ResourceCost:F1}");
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
        void Show(ITetrisItem item, ITetrisContainer container);
        void Hide();
    }
}