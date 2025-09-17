using HutongGames.PlayMaker;
using UnityEngine;
using UnityEngine.UIElements;

[RequireComponent(typeof(UIDocument))]
public class UITKSwipeDispatcher : MonoBehaviour
{
    [Tooltip("Minimum swipe distance in pixels before an event is fired.")]
    public float minSwipeDistance = 120f;

    [Tooltip("Fraction of screen width that counts as an activation edge.")]
    [Range(0f, 0.5f)]
    public float activationEdgePercent = 0.2f;

    [Tooltip("PlayMaker event sent when a swipe starts at the left edge and moves right.")]
    public string swipeRightEvent = "SWIPE_RIGHT";

    [Tooltip("PlayMaker event sent when a swipe starts at the right edge and moves left.")]
    public string swipeLeftEvent = "SWIPE_LEFT";

    [Tooltip("Optional event for upward swipes.")]
    public string swipeUpEvent = string.Empty;

    [Tooltip("Optional event for downward swipes.")]
    public string swipeDownEvent = string.Empty;

    private UIDocument _document;
    private VisualElement _root;

    private bool _tracking;
    private Vector2 _startPosition;
    private bool _allowLeft;
    private bool _allowRight;

    private void OnEnable()
    {
        _document = GetComponent<UIDocument>();
        AttachToRoot();
        if (_document?.rootVisualElement != null)
            _document.rootVisualElement.RegisterCallback<AttachToPanelEvent>(OnAttachToPanel);
    }

    private void OnDisable()
    {
        if (_document?.rootVisualElement != null)
            _document.rootVisualElement.UnregisterCallback<AttachToPanelEvent>(OnAttachToPanel);

        DetachFromRoot();
        _root = null;
        _tracking = false;
    }

    private void AttachToRoot()
    {
        var docRoot = _document != null ? _document.rootVisualElement : null;
        if (docRoot == null || docRoot == _root)
            return;

        DetachFromRoot();

        _root = docRoot;
        _root.RegisterCallback<PointerDownEvent>(OnPointerDown, TrickleDown.TrickleDown);
        _root.RegisterCallback<PointerUpEvent>(OnPointerUp, TrickleDown.TrickleDown);
        _root.RegisterCallback<PointerCancelEvent>(OnPointerCancel, TrickleDown.TrickleDown);
    }

    private void DetachFromRoot()
    {
        if (_root == null)
            return;

        _root.UnregisterCallback<PointerDownEvent>(OnPointerDown);
        _root.UnregisterCallback<PointerUpEvent>(OnPointerUp);
        _root.UnregisterCallback<PointerCancelEvent>(OnPointerCancel);
    }

    private void OnAttachToPanel(AttachToPanelEvent _)
    {
        AttachToRoot();
    }

    private void OnPointerDown(PointerDownEvent evt)
    {
        if (_root == null)
            return;

        var panelBounds = _root.panel?.visualTree?.resolvedStyle;
        var width = panelBounds?.width > 0f ? panelBounds.Value.width : Screen.width;
        var height = panelBounds?.height > 0f ? panelBounds.Value.height : Screen.height;
        if (width <= 0f || height <= 0f)
            return;

        var edgePixels = Mathf.Max(0f, activationEdgePercent) * width;

        _allowRight = evt.position.x <= edgePixels;
        _allowLeft = evt.position.x >= width - edgePixels;

        if (!_allowLeft && !_allowRight)
        {
            _tracking = false;
            return;
        }

        _tracking = true;
        _startPosition = evt.position;
    }

    private void OnPointerUp(PointerUpEvent evt)
    {
        if (!_tracking)
            return;

        _tracking = false;
        EvaluateSwipe(evt.position);
    }

    private void OnPointerCancel(PointerCancelEvent _)
    {
        _tracking = false;
    }

    private void EvaluateSwipe(Vector2 endPosition)
    {
        Vector2 delta = endPosition - _startPosition;
        if (delta.magnitude < minSwipeDistance)
            return;

        if (Mathf.Abs(delta.x) >= Mathf.Abs(delta.y))
        {
            if (_allowRight && delta.x > 0f && !string.IsNullOrEmpty(swipeRightEvent))
                PlayMakerFSM.BroadcastEvent(swipeRightEvent);
            else if (_allowLeft && delta.x < 0f && !string.IsNullOrEmpty(swipeLeftEvent))
                PlayMakerFSM.BroadcastEvent(swipeLeftEvent);
        }
        else
        {
            if (delta.y > 0f && !string.IsNullOrEmpty(swipeUpEvent))
                PlayMakerFSM.BroadcastEvent(swipeUpEvent);
            else if (delta.y < 0f && !string.IsNullOrEmpty(swipeDownEvent))
                PlayMakerFSM.BroadcastEvent(swipeDownEvent);
        }
    }
}
