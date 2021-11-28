using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SortingZone : MonoBehaviour
{
    public GameManager gm;

    public Interactable.SortingType id = Interactable.SortingType.ALL;

    public bool killsPlayer = false;

    private void Start()
    {
        transform.GetComponent<MeshRenderer>().enabled = false;
    }

    private void OnTriggerEnter(Collider other)
    {
        Interactable i;
        if (other.transform.TryGetComponent<Interactable>(out i))
        {
            if (i.sortID == id || i.sortID == Interactable.SortingType.ALL || id == Interactable.SortingType.ALL)
            {
                OnSort(i);
            }
            else
            {
                OnSortFail(i);
            }
        }
        else if (killsPlayer && other.transform.tag == "Player")
        {
            other.transform.GetComponent<Player>().Kill();
        }
    }

    private void OnSort(Interactable i)
    {
        Destroy(i.gameObject);
        gm.OnSuccess();
    }

    private void OnSortFail(Interactable i)
    {
        Destroy(i.gameObject);
        gm.OnFailure();
    }
}
