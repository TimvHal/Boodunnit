using Entities.Humans;
using Enums;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class IconCanvas : MonoBehaviour
{
    [HideInInspector]
    public GameObject IconTarget;
    public GameObject IconTargetDashable;

    public int ImageWidth;
    public int ImageHeight;

    public GridLayoutGroup GridLayoutGroup;
    public RectTransform GridLayoutTransform;

    public Image[] IconImages;

    public Image DashIconImage;

    private List<Image> _enabledIconImages = new List<Image>();

    private void Awake()
    {
        if (IconImages != null && GridLayoutTransform && GridLayoutGroup)
        {
            GridLayoutTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, ImageHeight);
            GridLayoutGroup.cellSize = new Vector2(ImageWidth, ImageHeight);

            foreach (Image iconImage in IconImages)
            {
                RectTransform imageTransform = iconImage.gameObject.GetComponent<RectTransform>();
                if (imageTransform)
                {
                    imageTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, ImageWidth);
                    imageTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, ImageHeight);
                }
            }
            RectTransform imageTransformDashIcon = DashIconImage.gameObject.GetComponent<RectTransform>();
            if (imageTransformDashIcon)
            {
                imageTransformDashIcon.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, ImageWidth);
                imageTransformDashIcon.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, ImageHeight);
            }
        }
    }

    // Update is called once per frame
    void Update()
    {
        UpdateImagePosition();
        UpdateDashImagePosition();
    }

    public void DisableIcons()
    {
        foreach (Image iconImage in _enabledIconImages)
        {
            iconImage.gameObject.SetActive(false);
            iconImage.transform.SetParent(transform, false);
        }
        _enabledIconImages.Clear();
        DashIconImage.gameObject.SetActive(false);
    }

    public void EnableIcons()
    {
        IIconable iconable = IconTarget.GetComponent<IIconable>();
        if (iconable != null)
        {
            if (iconable.GetEmotionalState() == EmotionalState.Fainted)
            {
                EnableIcon(WorldIconType.Ragdoll);
            }

            else if (PossessionBehaviour.PossessionTarget)
            {
                EnableIcon(WorldIconType.TalkTo);
                EnableIconEmotionalStates(iconable);
            }
            else if (!PossessionBehaviour.PossessionTarget)
            {
                if (iconable.GetCanBePossessed())
                {
                    EnableIcon(WorldIconType.Possess);
                }
                if (iconable.GetCanTalkToBoolia())
                {
                    EnableIcon(WorldIconType.TalkTo);
                }
                EnableIconEmotionalStates(iconable);
            }
        }
        else if (IconTarget.GetComponent<ILevitateable>() != null)
        {
            EnableIcon(WorldIconType.Levitate);
        }
        else if (IconTarget.GetComponent<WorldSpaceClue>() != null)
        {
            EnableIcon(WorldIconType.PickupClue);
        }
        else if (PossessionBehaviour.PossessionTarget)
        {
            if (IconTarget.GetComponent<AirVent>() != null && PossessionBehaviour.PossessionTarget.GetComponent<BirdBehaviour>() != null)
            {
                EnableIcon(WorldIconType.BirdGlide);
            }
            else if (IconTarget.layer == 12 && PossessionBehaviour.PossessionTarget.GetComponent<RatBehaviour>() != null)
            {
                EnableIcon(WorldIconType.RatClimb);
            }
        }
    }

    private void EnableIconEmotionalStates(IIconable iconable)
    {
        if (iconable.GetEmotionalState() == EmotionalState.Calm)
        {
            EnableIcon(WorldIconType.Normal);
        }
        else if (iconable.GetEmotionalState() == EmotionalState.Scared)
        {
            EnableIcon(WorldIconType.Scared);
        }
        else if (iconable.GetEmotionalState() == EmotionalState.Terrified)
        {
            EnableIcon(WorldIconType.Terrified);
        }
    }

    private void EnableIcon(WorldIconType iconType)
    {
        foreach (Image iconImage in IconImages)
        {
            if (iconImage.name.Contains(iconType.ToString()))
            {
                AddImageToEnabledList(iconImage);
                iconImage.transform.SetParent(GridLayoutGroup.transform, false);
                UpdateImagePosition();
                iconImage.gameObject.SetActive(true);
            }
        }
    }

    public void EnableDashIcon()
    {
        if (IconTargetDashable.layer == 10)
        {
            DashIconImage.gameObject.SetActive(true);
            UpdateDashImagePosition();
        }
    }

    private void AddImageToEnabledList(Image iconImage)
    {
        if (!_enabledIconImages.Contains(iconImage))
        {
            _enabledIconImages.Add(iconImage);
        }
    }

    private void UpdateDashImagePosition()
    {
        if (_enabledIconImages != null && IconTargetDashable != null)
        {
            Vector3 iconTargetPos = IconTargetDashable.transform.position;

            DashIconImage.GetComponent<RectTransform>().position = Camera.main.WorldToScreenPoint(iconTargetPos);
        }
    }

    private void UpdateImagePosition()
    {
        if (_enabledIconImages != null && IconTarget != null)
        {
            GridLayoutTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, CalculateGridWidth());

            Vector3 iconTargetPos = IconTarget.transform.position;

            if (IconTarget.GetComponent<AirVent>() == null && IconTarget.gameObject.layer != 12)
            {
                iconTargetPos.y += IconTarget.GetComponent<Collider>().bounds.extents.y;
            }

            GridLayoutTransform.position = Camera.main.WorldToScreenPoint(iconTargetPos);
        }
    }

    private int CalculateGridWidth()
    {
        int listSize = _enabledIconImages.Count;
        if (listSize > 0)
        {
            return (listSize * ImageWidth) + ((listSize - 1) * 5);
        }
        return 0;
    }
}
