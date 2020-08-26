using Crest;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.VFX;

public class CannonballController : DestroyableBase
{
    public HashSet<Collider> doNotCollide;
    public Rigidbody rb;
    public float damage = 30f;
    public float lifespan = 10f;
    SampleHeightHelper _sampleHeightHelper = new SampleHeightHelper();
    public AudioClipGroup splashOptions;
    public VisualEffect trailEffect;

    private void OnTriggerEnter(Collider collider)
    {
        if (!doNotCollide.Contains(collider) && !isDestroying && !collider.isTrigger)
        {
            var health = collider.gameObject.GetComponentInParent<Health>();
            if (health != null)
            {
                health.DoDamage(transform.position, rb, damage);
            }
            DestroyAfterAudio(gameObject, null);
        }
    }

    void Update()
    {
        _sampleHeightHelper.Init(transform.position);
        float oceanHeight = 1f;
        _sampleHeightHelper.Sample(ref oceanHeight);
        if (!isDestroying && (lifespan <= 0 || transform.position.y < -20 || transform.position.y < oceanHeight - 1f))
        {
            DestroyAfterAudio(gameObject, splashOptions.Get());
        }
        lifespan -= Time.deltaTime;
        trailEffect.SetVector3("Position", transform.position);
    }

    void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawSphere(transform.position, .5f);
    }
}
