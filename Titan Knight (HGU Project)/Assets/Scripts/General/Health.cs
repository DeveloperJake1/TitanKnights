using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum ObjectType
{
    NotSpecified,
    Enemy,
    Turret,
    Player
}


[Tooltip("This class is to be put onto any object that has health.")]
public class Health : MonoBehaviour 
{
    [Tooltip("This will turn RED when this instance takes damage!")][SerializeField] MeshRenderer modelMaterial;
    [Space(10)]
    [Tooltip("The start player health. (Not used to change/read current health!)")] [SerializeField] public int startHealth = 100;
    [Tooltip("The current health of the player. (Read only!)")] public int currentHealth = 1;
    
    [Tooltip("This is a test of adding resistance to the player. This can be used later to make 'Armor-like' buffs. (health - (damage / resistance))")] public int resistance;


    public ObjectType HealthType;

    private void Start()
    {
        if (HealthType == ObjectType.Turret) Debug.LogWarningFormat("Health Type is Turret, however, Turret health has no true functionality!");


        standardColor = modelMaterial.material.color;
        currentHealth = startHealth; // Initialize current health 
    }

    private void Update()
    {
        if (HealthType == ObjectType.NotSpecified) { Debug.LogErrorFormat("Please specify the Health Type of this health object."); return; }



        if (currentHealth <= 0) // This will kill the health object. If it's an enemy, it should be attatched to the parent object.
        {
            if (HealthType == ObjectType.Player) GetComponent<PlayerManager>().Die();
            else Die();

            this.enabled = false;
        }

    }

    public void Damage(int amount) // This can be called to damage this instance. (TAKE DAMAGE)
    {
        StopCoroutine(DamageRenderer());
        StartCoroutine(DamageRenderer());

        if (resistance == 0) currentHealth -= amount;

        else
        {
            currentHealth = Mathf.RoundToInt(currentHealth -= amount / resistance); // This just divides the damage by the resistance. It's a simple feature, but it works.
        }
    }

    private Color standardColor;

    private IEnumerator DamageRenderer() // This simply makes the renderer appear red for a tenth of a second when it gets damaged. Sounds can be added later.
    {

        modelMaterial.material.color = Color.red; 

        yield return new WaitForSeconds(0.1f);

        modelMaterial.material.color = standardColor;

    }



    private void Die() // This is called for enemies. Turret functionality is not going to be created.
    {
        if (HealthType != ObjectType.Enemy) return;

        // Cash.AddCash(value) functionality can be added in the MVP

        Destroy(this.gameObject); 
    }
}
