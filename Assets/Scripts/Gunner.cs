
using System;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(SphereCollider))]
[RequireComponent(typeof(LineRenderer))]
[RequireComponent(typeof(AudioSource))]
public class Gunner : MonoBehaviour
{
    public string TagEnemy;
    public GameObject Notch;
    public GameObject Horizontal;
    public GameObject Vertical;
    public float TurnSpeed = 3;
    public AudioClip ShootSound;
    public AudioClip ReloadSound;
    public Animator BarrelAnimator;
    public ParticleSystem Sparks;

    private readonly List<GameObject> enemies = new List<GameObject>();
    private float attackRadius;
    private LineRenderer laser;

    private enum State { Ready, Aming, Shooting, Reloading};
    private State actualState = State.Ready;
    private AudioSource audioSource;

    private GameObject enemy;
    
    // Start is called before the first frame update
    void Start()
    {
        attackRadius = GetComponent<SphereCollider>().radius;
        laser = GetComponent<LineRenderer>();
        audioSource = GetComponent<AudioSource>();
        laser.enabled = false;
        laser.SetPosition(0, Notch.transform.position);
    }

    // Update is called once per frame
    void Update()
    {
        ActualizeEnemy();

        GameObject enemy = ClosestEnemy;
        switch (actualState)
        {
            case State.Ready:
                if (enemy == null)
                    break;
                actualState = State.Aming;
                break;
            case State.Aming:
                if (enemy == null)
                {
                    actualState = State.Ready;
                    break;
                }
                if (!AimAt(enemy))
                    break;

                ShootAt(enemy);
                audioSource.PlayOneShot(ShootSound);
                actualState = State.Shooting;
                BarrelAnimator.ResetTrigger("CeaseFire");
                BarrelAnimator.SetTrigger("Shoot");
                Sparks.transform.position = laser.GetPosition(1);
                Sparks.Play();
                break;
            case State.Shooting:
                AimAt(enemy);
                if (audioSource.isPlaying)
                    break;
                actualState = State.Reloading;
                audioSource.PlayOneShot(ReloadSound);
                laser.enabled = false;
                BarrelAnimator.ResetTrigger("Shoot");
                BarrelAnimator.SetTrigger("CeaseFire");
                Sparks.Stop();
                break;
            case State.Reloading:
                AimAt(enemy);
                if (audioSource.isPlaying)
                    break;
                actualState = State.Ready;
                break;
            default:
                print("Unreachable State reached");
                break;
        }
    }

    private void ActualizeEnemy()
    {
        if (enemy == null || enemy.GetComponent<ComradController>().IsDying)
            enemy = ClosestEnemy;
    }

    void OnTriggerEnter(Collider collider)
    {
        GameObject enemy = collider.gameObject;
        if (collider.CompareTag(TagEnemy) && !enemies.Contains(enemy))
        {
            enemies.Add(enemy);
        }
            
        
    }

    void OnTriggerExit(Collider collider)
    {
        GameObject enemy = collider.gameObject;
        if (enemies.Contains(enemy))
            enemies.Remove(enemy);
    }

    private void ShootAt(GameObject enemy)
    {
        Vector3 headPosition = enemy.GetComponent<ComradController>().Head.transform.position;

        laser.SetPosition(1, headPosition);
        laser.enabled = true;

        ComradController comradController = enemy.GetComponent<ComradController>();
        comradController.TakeDamage(10f);
    }

    private bool AimAt(GameObject enemy)
    {
        if (enemy == null)
            return false;

        Vector3 headPosition = enemy.GetComponent<ComradController>().Head.transform.position;
        laser.SetPosition(1, headPosition);

        Vector3 direction = headPosition - Notch.transform.position;

        Quaternion rotation = Quaternion.LookRotation(direction);

        Quaternion horizontalRotation = 
            new Quaternion { eulerAngles = new Vector3(0, rotation.eulerAngles.y, 0) };
        Horizontal.transform.rotation = 
            Quaternion.Lerp(Horizontal.transform.rotation, horizontalRotation, Time.deltaTime * TurnSpeed);

        Quaternion verticalRotation = new Quaternion { eulerAngles = 
            new Vector3(rotation.eulerAngles.x + 90, Horizontal.transform.eulerAngles.y, Horizontal.transform.eulerAngles.z) };
        Vertical.transform.rotation = 
            Quaternion.Lerp(Vertical.transform.rotation, verticalRotation, Time.deltaTime * TurnSpeed); 

        DrawRay(enemy);

        float hPrecision = Quaternion.Dot(Vertical.transform.rotation, verticalRotation);
        float vPrecision = Quaternion.Dot(Horizontal.transform.rotation, horizontalRotation);

        return Mathf.Abs(hPrecision) > 0.9995f && Mathf.Abs(vPrecision) > 0.9995f;
    }

    private void DrawRay(GameObject enemy)
    {
        Vector3 headPosition = enemy.GetComponent<ComradController>().Head.transform.position;

        Vector3 direction = headPosition - Notch.transform.position;
        //Debug.DrawRay(Notch.transform.position, direction);
    }

    private GameObject ClosestEnemy
    {
        get
        {
            enemies.RemoveAll(enemy => enemy == null || enemy.GetComponent<ComradController>().IsDying);

            GameObject closestEnemy = null;
            float minDistance = float.MaxValue;
            foreach (GameObject enemy in enemies)
            {
                Vector3 direction = enemy.transform.position - Notch.transform.position;
                RaycastHit hit;
                if (!Physics.Raycast(Notch.transform.position, direction, out hit, attackRadius))
                    continue;

                if (hit.transform.gameObject != enemy)
                    continue;

                float distance = direction.magnitude;
                if (distance < minDistance)
                {
                    minDistance = distance;
                    closestEnemy = enemy;
                }
            }

            return closestEnemy;
        }
    }
}
    