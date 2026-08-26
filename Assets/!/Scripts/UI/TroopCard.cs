using NUnit.Framework;
using System;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class TroopCard : MonoBehaviour, IBeginDragHandler, IEndDragHandler, IDragHandler
{
    [SerializeField] private TroopSO _troopSO;

    [Header("References")]
    [SerializeField] private Image _cardArt;
    [SerializeField] private TMP_Text _cardName;
    [SerializeField] private GameObject _dragIcon;
    [SerializeField] private GameObject _lock;
    [SerializeField] private TMP_Text _cost;
    
    private TroopData _data;
    private bool _isLocked;

    private void Awake()
    {
        _data = new TroopData(_troopSO);
        _cardArt.sprite = _data.cardArt;
        _cardName.text = _data.displayName;

        _dragIcon.SetActive(false);
    }

    void Start()
    {
        if (_data != null)
        {
            // Do something with the troop data
            if (_data.cardArt != null)
            {
                _cardArt.sprite = _data.cardArt;
            }
            _cardName.text = _data.displayName;
        }

        if(LevelManager.Instance != null)
        {
            VerifyAffordability(LevelManager.Instance.GetCurrentMoney());
            LevelManager.Instance.OnMoneyChanged += VerifyAffordability;
        }
    }

    private void LockCard(bool isLocked)
    {
        _lock.SetActive(isLocked);
        _isLocked = isLocked;
    }

    private void VerifyAffordability(int currentMoney)
    {
        LockCard(_troopSO.cost > currentMoney);
    }


    public void OnBeginDrag(PointerEventData eventData)
    {
        if (_isLocked) return;

        _dragIcon.SetActive(true);
        TroopSlot[] slots = FindObjectsByType<TroopSlot>();

        foreach (TroopSlot slot in slots)
        {
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
            Debug.Log($"Hit: {hit.collider.name}");

            TroopSlot slot = hit.collider.GetComponent<TroopSlot>();
            if (slot != null)
            {
                slot.SetTroopData(_data);
                LevelManager.Instance.RemoveMoney(_troopSO.cost);
            }
        }
    }
    
}
