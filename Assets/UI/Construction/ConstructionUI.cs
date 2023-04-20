using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UIElements;
using VoxelPlay;

public class ConstructionUI : UIHandler
{
    [SerializeField] UIDocument uiDocument;
    [SerializeField] DetachedCamera detachedCamera;
    [SerializeField] InputManager inputManager;
    [SerializeField] VoxelWorldManager voxelWorldManager;

    public ConstructionOptions constructionOptions { get; private set; }

    private VisualElement constructionUIRoot;
    private ConstructionBox constructionBox;
    private float currNavigationDirection = 0;
    private Coroutine navigationCoroutine = null;

    public const string CONSTRUCTION_UI_ROOT = "ConstructionUI";

    void Awake() {
        constructionUIRoot = uiDocument.rootVisualElement.Q<VisualElement>(CONSTRUCTION_UI_ROOT);
        constructionBox = constructionUIRoot.Q<ConstructionBox>();
    }

    public void OnEnvInitialized() {
        List<VoxelDefinition> voxelDefinitions = voxelWorldManager.GetEnvironment().voxelDefinitions.ToList();
        constructionOptions = new ConstructionOptions(voxelDefinitions, new List<GameObject>());
        constructionBox.RenderOptions(constructionOptions);
    }

    public void ApplyFocus() {
        constructionBox.topOption.Focus();
    }

    public override void SetDisplayState(bool isVisible) {
        if (isVisible) {
            constructionUIRoot.style.display = DisplayStyle.Flex;
        }
        else {
            constructionUIRoot.style.display = DisplayStyle.None;
        }
    }

    public override void HandleNavigate(InputAction.CallbackContext obj) {
        Vector2 stickValue = obj.ReadValue<Vector2>();
        currNavigationDirection = stickValue.x;
        if (stickValue.x == 0) {
            CancelNavigateOptions();
            return;
        }
        if (navigationCoroutine == null) {
            navigationCoroutine = StartCoroutine(ExecuteNavigateOptions());
        }
    }

    public override void HandleCancelNavigate(InputAction.CallbackContext obj) {
        CancelNavigateOptions();
    }

    private void CancelNavigateOptions() {
        currNavigationDirection = 0;
        if (navigationCoroutine != null) {
            StopCoroutine(navigationCoroutine);
            navigationCoroutine = null;
        }
    }

    private IEnumerator ExecuteNavigateOptions() {
        while (currNavigationDirection != 0) {
            bool isRight = currNavigationDirection > 0;
            if (uiDocument.rootVisualElement.focusController.focusedElement == constructionBox.topOption) {
                constructionOptions.IterateTop(isRight);
            }
            else {
                constructionOptions.IterateBottom(isRight);
            }
            constructionBox.RenderOptions(constructionOptions);

            yield return new WaitForSeconds(0.25f);
        }

        navigationCoroutine = null;
    }

    public override void HandleSubmit(InputAction.CallbackContext obj) {
        detachedCamera.HandleSwitchFromUIToDetached();
    }
    public override void HandleCancel(InputAction.CallbackContext obj) {
        detachedCamera.HandleSwitchFromUIToDetached();
    }
}
