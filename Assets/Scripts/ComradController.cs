using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(Animator))]
[RequireComponent(typeof(NavMeshAgent))]
[RequireComponent(typeof(ParticleSystem))]
public class ComradController : MonoBehaviour
{
    public string TargetName;
    public int TurnSpeed = 5;
    public float InitialHealth = 30f;
    public bool IsDying { get; private set; }
    public GameObject Head;

    private Animator animator;
    private NavMeshAgent agent;
    private GameObject target;
    private Transform targetPosition;
    private ParticleSystem steam;
    private float health;
    private SceneController sceneController;



    void Start()
    {
        animator = GetComponent<Animator>();
        agent = GetComponent<NavMeshAgent>();
        steam = GetComponent<ParticleSystem>();
        sceneController = GameObject.Find("SceneController").GetComponent<SceneController>();
        Steam(false);

        agent.updatePosition = false;
        agent.updateRotation = false;

        target = GameObject.Find(TargetName);

        health = InitialHealth;

        if (target != null)
        {
            targetPosition = target.transform;
            agent.SetDestination(targetPosition.position);
            animator.SetTrigger("Walk");
        }
    }

    void Update()
    {
        if (targetPosition == null)
            return;

        CheckHealth();

        Approach();
    }

    private void CheckHealth()
    {
        if( health < 0 && !IsDying)
        {
            Destroy(gameObject, 2);
            animator.SetTrigger("Die");
            IsDying = true;
            ParticleSystem.EmissionModule emission = steam.emission;
            emission.rateOverTime = 50;
            sceneController.ReportIncident(SceneController.Incident.PlayerKilled);
        } else if (health < InitialHealth / 3)
        {
            Steam(true);
        }
    }

    private void Steam(bool status)
    {
        if (!steam.isPlaying && status)
            steam.Play();

        if (steam.isPlaying && !status)
            steam.Stop();
    }

    private void Approach()
    {
        Vector3 direction = agent.steeringTarget - transform.position;
        direction.y = 0;

        Quaternion rotation = Quaternion.LookRotation(direction);
        transform.rotation = Quaternion.Lerp(transform.rotation, rotation, Time.deltaTime * TurnSpeed);
        transform.position += direction.normalized * animator.deltaPosition.magnitude;

        agent.nextPosition = transform.position;
    }

    public void TakeDamage(float damage)
    {
        health -= damage;
        print(health);
    }
     
}
