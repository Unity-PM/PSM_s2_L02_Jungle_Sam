using UnityEngine;

[CreateAssetMenu(menuName = "Jungle Sam/UI/Story Popup Data", fileName = "POPUP_StoryData")]
public class StoryPopupData : ScriptableObject
{
    [SerializeField] private string popupId = "POPUP_StoryData";
    [SerializeField] private Sprite itemImage;
    [SerializeField] private string categoryLabel = "DOWÓD / ŁUP FABULARNY";
    [SerializeField] private string title = "TYTUL";
    [SerializeField] private string subtitle = "Podtytuł";
    [TextArea(4, 14)]
    [SerializeField] private string body;
    [SerializeField] private string continueButtonText = "KONTYNUUJ";
    [SerializeField] private bool lockPlayerWhileOpen = true;

    public string PopupId => popupId;
    public Sprite ItemImage => itemImage;
    public string CategoryLabel => categoryLabel;
    public string Title => title;
    public string Subtitle => subtitle;
    public string Body => body;
    public string ContinueButtonText => continueButtonText;
    public bool LockPlayerWhileOpen => lockPlayerWhileOpen;
}
