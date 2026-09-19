using NUnit.Framework;
using System;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class TroopCard : MonoBehaviour, IBeginDragHandler, IEndDragHandler, IDragHandler
{
    [Header("References")]
    [SerializeField] private Image _cardArt;
    [SerializeField] private TMP_Text _cardName;
    [SerializeField] private TMP_Text _levelText;
    [SerializeField] private GameObject _dragIcon;
    [SerializeField] private GameObject _lock;
    [SerializeField] private TMP_Text _cost;

    [Header("Audio")]
    [SerializeField] private AudioClip _cardClickSound;

    private TroopData _data;
    private bool _isLocked;

    void Start()
    {
        LevelManager.Instance.OnMoneyChanged += VerifyAffordability;
        _dragIcon.SetActive(false);
    }

    private void LockCard(bool isLocked)
    {
        _lock.SetActive(isLocked);
        _isLocked = isLocked;
    }

    private void VerifyAffordability(int currentMoney)
    {
        LockCard(_data.cost > currentMoney);
    }

    public void SetupCard(TroopData data)
    {
        if (data != null)
        {
            _data = data;
            // Do something with the troop data
            if (data.cardArt != null)
            {
                _cardArt.sprite = data.cardArt;
            }
            _cardName.text = data.displayName;
            _cost.text = _data.cost.ToString();
            VerifyAffordability(LevelManager.Instance.GetCurrentMoney());
            UpdateLevelText(data);

            Debug.Log($"TroopCard Setup: Name={data.displayName}, Level={data.level}, Cost={_data.cost}");
        }
    }

    private void UpdateLevelText(TroopData data)
    {
        if (_levelText != null && data != null)
        {
            _levelText.text = $"Lv. {data.level}";
        }
    }
    private void OnDestroy()
    {
        if (LevelManager.Instance != null)
        {
            LevelManager.Instance.OnMoneyChanged -= VerifyAffordability;
        }
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        if (_isLocked) return;
        
        AudioManager.Instance.PlaySFX(_cardClickSound);

        _dragIcon.SetActive(true);
        TroopSlot[] slots = FindObjectsByType<TroopSlot>();

        foreach (TroopSlot slot in slots)
        {
            if(slot.GetTroopSlotType() == _data.troopSlotType)
                slot.ShowFreeSlot();
        }
    }

    public void OnDrag(PointerEventData eventData)
    {
        if (_isLocked) return;

        _dragIcon.transform.position = eventData.position;
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        if(_isLocked) return;

        _dragIcon.SetActive(false);

        TroopSlot[] slots = FindObjectsByType<TroopSlot>();

        foreach (TroopSlot slot in slots)
        {
            slot.HideFreeSlot();
        }

        RaycastHit hit;
        Physics.Raycast(Camera.main.ScreenPointToRay(eventData.position), out hit, Mathf.Infinity, LayerMask.GetMask("TroopSlot"));

        if (hit.collider != null)
        {
            TroopSlot slot = hit.collider.GetComponent<TroopSlot>();
            if (slot != null)
            {
                if(!slot.IsOccupied && slot.GetTroopSlotType() == _data.troopSlotType)
                {
                    slot.SetTroopData(_data);
                    LevelManager.Instance.RemoveMoney(_data.cost);
                }

            }
        }
    }
    
}
