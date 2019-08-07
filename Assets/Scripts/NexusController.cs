using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(ParticleSystem))]

public class NexusController : MonoBehaviour
{
    public string EnemyTag;

    private ParticleSystem boost;
    private SceneController sceneController;

    void Start()
    {
        boost = GetComponent<ParticleSystem>();
        sceneController = GameObject.Find("SceneController").GetComponent<SceneController>();
    }

    private void OnTriggerEnter(Collider collider)
    {
        if (collider.gameObject.CompareTag(EnemyTag))
        {
            boost.Emit(50);
            Destroy(collider.gameObject);
            sceneController.ReportIncident(SceneController.Incident.PlayerReachedNexus);
        }
    }
}
