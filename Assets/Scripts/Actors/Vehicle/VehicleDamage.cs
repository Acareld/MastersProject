using System;
using UnityEngine;

namespace Vehicle
{

    [Serializable]
    public struct DamageData
    {
        public float segmentDamageImpact;
        public float segmentPeakSeverity;
        public int segmentImpacts;

        public void Reset()
        {
            segmentDamageImpact = 0;
            segmentPeakSeverity = 0;
            segmentImpacts = 0;
        }
    }

    public class VehicleDamage : MonoBehaviour
    {
        private Rigidbody rb;

        [SerializeField]
        public float maxHealth = 100f;
        private AnimationCurve damageCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);

        public float currentHealth { get; private set; }

        private DamageData damageData = new DamageData();

        public event Action<float> noHealthAction;

        public event Action<float> damageReceivedAction;

        private bool bCanTakeDamage = false;


        [SerializeField]
        private LayerMask collisionLayer;

        private void Awake()
        {
            rb = GetComponent<Rigidbody>();
            currentHealth = maxHealth;
        }

        private void OnCollisionEnter(Collision collision)
        {
            if (!bCanTakeDamage) return;

            if (!IsInlayerMask(collision.gameObject.layer, collisionLayer)) return;
            if (collision.contactCount == 0) return;

            ContactPoint contact = collision.GetContact(0);

            float impactDeltaVel = collision.impulse.magnitude / Mathf.Max(rb.mass, 0.001f);

            if (impactDeltaVel < 3f)
            {
                Debug.Log("Too slow for damage");
                return;
            }

            float severity = Mathf.InverseLerp(3f, 6f, impactDeltaVel);

            float damage = 30f * damageCurve.Evaluate(severity);

            Debug.Log("Damage Done: impactDeltaV: " + impactDeltaVel + ", severity: " + severity + ", damage: " + damage);

            ApplyDamage(damage);

            damageData.segmentDamageImpact += severity * severity;
            damageData.segmentPeakSeverity = Mathf.Max(damageData.segmentPeakSeverity, severity);
            damageData.segmentImpacts++;


        }

        private void ApplyDamage(float amount)
        {
            if (amount <= 0f) return;

            currentHealth = Mathf.Max(0f, currentHealth - amount);

            damageReceivedAction?.Invoke(currentHealth);

            if (currentHealth == 0f)
            {
                Debug.LogWarning("VEHICLE DONE FOR TODAY, too much damage");
                noHealthAction?.Invoke(amount);
            }
        }

        public void Disable()
        {
            bCanTakeDamage = false;
        }

        public void Enable()
        {
            bCanTakeDamage = true;
        }


        public DamageData CollectAndResetDataCollection()
        {
            DamageData temp = damageData;
            damageData.Reset();
            return temp;
        }

        public void ResetHealth()
        {
            currentHealth = maxHealth;
        }


        private static bool IsInlayerMask(int layer, LayerMask mask)
        {
            return (mask.value & (1 << layer)) != 0;
        }

        // Start is called once before the first execution of Update after the MonoBehaviour is created
        void Start()
        {

        }

        // Update is called once per frame
        void Update()
        {

        }
    }
}