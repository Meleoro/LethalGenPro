using System;
using UnityEngine;

public class PlayerStart : MonoBehaviour
{
    [SerializeField] private GameObject playerPrefab;

    private void Start()
    {
        Instantiate(playerPrefab, transform.position, Quaternion.identity);
    }
}
