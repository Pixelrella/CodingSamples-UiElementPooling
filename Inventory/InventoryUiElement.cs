using foo.ui.performance;

namespace foo.ui.inventory
{
  public class InventoryUiElement : MonoBehaviour, IClearable
  {
    [SerializeField] private Transform iconRoot;

    private TextMeshProUGUI headerLabel;
    private IconUiElement iconPrefab;

    public void Init(IconUiElement iconPrefab)
    {
      headerLabel = GetComponentInChildren<TextMeshProUGUI>(); // Very silly setup to demonstrate the current drawback of calling init when the element is made visible.
      this.iconPrefab = iconPrefab;
    }

    public void Show(InventoryItem item)
    {
      headerLabel.text = item.name;
      var icon = Object.Instantiate<IconUiElement>(iconPrefab, iconRoot);
      icon.Show(item);
    }

    public void Clear()
    {
      GameObject.Destroy(iconRoot.GetChild(0).gameObject);
    }
  }
}
