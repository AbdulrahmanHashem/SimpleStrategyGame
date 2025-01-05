using System;
using TMPro;
using Unity.Collections;
using Unity.Entities;
using UnityEngine;
using UnityEngine.UI;

public class UnitSelectionManagerUI : MonoBehaviour
{
    public PlayerInput playerInput;
    [SerializeField] private RectTransform selectionAreaRectTransform;
    
    private UnitSelectionManager _unitSelectionManager;
    
    public Canvas canvas;
    public Button aggressionButton;
    
    private EntityManager _entityManager;
    
    private void Awake()
    {
        _unitSelectionManager = UnitSelectionManager.Instance;
        playerInput = PlayerInput.Instance;
    }
    
    private void Start()
    {
        _entityManager = World.DefaultGameObjectInjectionWorld.EntityManager;
        
        selectionAreaRectTransform.gameObject.SetActive(false);

        aggressionButton.onClick.AddListener(TurnPassive);
    }
    
    private void Update()
    {
        if (selectionAreaRectTransform.gameObject.activeSelf)
        {
            UpdateVisual();
        }
    }

    private void OnEnable()
    {
        _unitSelectionManager.OnSelectionAreaStart += UnitSelectionStart;
        _unitSelectionManager.OnSelectionAreaEnd += UnitSelectionEnd;
        _unitSelectionManager.OnUnitsSelected += OnUnitsSelected;
    }

    private void OnDisable()
    {
        _unitSelectionManager.OnSelectionAreaStart -= UnitSelectionStart;
        _unitSelectionManager.OnSelectionAreaEnd -= UnitSelectionEnd;
        _unitSelectionManager.OnUnitsSelected -= OnUnitsSelected;
    }

    private void UnitSelectionStart(object sender, EventArgs e)
    {
        selectionAreaRectTransform.gameObject.SetActive(true);
        UpdateVisual();
    }
    
    private void UnitSelectionEnd(object sender, EventArgs e)
    {
        selectionAreaRectTransform.gameObject.SetActive(false);
    }

    private void UpdateVisual()
    {
        Rect selectionAreaRect = _unitSelectionManager.GetSelectionArea();
        
        float canvasScale = canvas.transform.localScale.x;
        
        selectionAreaRectTransform.anchoredPosition = new Vector2(selectionAreaRect.x, selectionAreaRect.y) / canvasScale;
        
        selectionAreaRectTransform.sizeDelta =
            new Vector2(selectionAreaRect.width, selectionAreaRect.height) / canvasScale;
    }
    
    private void OnUnitsSelected(object sender, NativeArray<Entity> entities)
    {
        foreach (var entity in entities)
        {
            if (_entityManager.HasComponent<Detector>(entity))
            {
                // enable button
                aggressionButton.enabled = true;
                return;
            }
        }
        // disable button
        aggressionButton.enabled = false;
    }

    public void TurnAggressive()
    {
        EntityQuery aggressionQuery = new EntityQueryBuilder(Allocator.Temp)
            .WithAll<Selected>()
            .WithPresent<Detector>()
            .Build(_entityManager);
        
        NativeArray<Entity> entities = aggressionQuery.ToEntityArray(Allocator.Temp);

        foreach (var entity in entities)
        {
            if (!_entityManager.IsComponentEnabled<Detector>(entity))
            {
                _entityManager.SetComponentEnabled<Detector>(entity, true);
            }
        }

        aggressionButton.GetComponentInChildren<TextMeshProUGUI>().text = "Aggressive";
        aggressionButton.onClick.RemoveListener(TurnAggressive);
        aggressionButton.onClick.AddListener(TurnPassive);
    }    
    
    public void TurnPassive()
    {
        EntityQuery aggressionQuery = new EntityQueryBuilder(Allocator.Temp)
            .WithAll<Selected, Detector>()
            .Build(_entityManager);
        
        NativeArray<Entity> entities = aggressionQuery.ToEntityArray(Allocator.Temp);
        
        foreach (var entity in entities)
        {
            _entityManager.SetComponentEnabled<Detector>(entity, false);
        }
        
        aggressionButton.GetComponentInChildren<TextMeshProUGUI>().text = "Passive";
        aggressionButton.onClick.RemoveListener(TurnPassive);
        aggressionButton.onClick.AddListener(TurnAggressive);
    }
    
    public void SetPointerOverUI(bool pointerOverUI) => playerInput.SetPointerOverUI(pointerOverUI);
}