using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UIManager : MonoBehaviour
{
    [Header("Stats")]
    [SerializeField] private UserStats stats;

    [Header("Img")]
    [SerializeField] private Image Icon;

    [Header("Text")]
    [SerializeField] private TextMeshProUGUI worldrank;
    [SerializeField] private TextMeshProUGUI id;


    private void Update()
    {
        UpdataPlayerUI();
    }

    private void UpdataPlayerUI()
    {
        id.text = $"User ID: {stats.id}";
        worldrank.text = $"World Rank: {stats.worldrank}";
        Icon.sprite = stats.icon;
    }
}