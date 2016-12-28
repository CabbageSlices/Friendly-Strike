using UnityEngine;
using System.Collections;
using System.Collections.Generic;

//handles all the behaviour for a given type of bullet
//i.e movement
public class BulletBehaviour : MonoBehaviour {

    //reference to teamManager to get colliders for players to ignore collision with teamamates of shooter
    [System.NonSerialized]public TeamManager teamManager;
    
    //id of the team that fired the bullet
    [System.NonSerialized]
    public TeamProperties.Teams idTeamThatFiredBullet;

    //property of the bullet this behaviour refers to
    public BulletProperties property;
    public Rigidbody2D rigidBody;

    //bullets collider cached
    [SerializeField]private BoxCollider2D boxCollider;

	// Use this for initialization
	void Start () {

        if (rigidBody == null)
            Debug.Log("rigidBody in BulletBehaviour is null");

        if (property == null)
            Debug.Log("Bullet property is null");
	}

    //when gun is fired it will create a bullet
    //this function will use the given parameters to determine the bullets movement
    //gunAngle is the angle of the gun IN RADIANS
    //gunAimDeviation is how much a bullet should strayf rom the intended gunAngle IN DEGREES
    public void fire(Vector3 startingPosition, float gunAngle, float gunAimDeviation) {

        transform.position = startingPosition;

        float actualFireAngle = Mathf.Rad2Deg * gunAngle + Random.Range(-gunAimDeviation, gunAimDeviation);

        //make bullet point in the same direction as the gun
        transform.rotation = Quaternion.Euler(0, 0, actualFireAngle);

        rigidBody.velocity = Quaternion.Euler(0, 0, actualFireAngle) * Vector2.right * property.speed;

        setupCollisionsToIgnore();
    }

    void setupCollisionsToIgnore() {

        List<BoxCollider2D> teammateColliders = teamManager.collidersForEachTeam[idTeamThatFiredBullet];

        foreach (BoxCollider2D collider in teammateColliders)
            Physics2D.IgnoreCollision(collider, boxCollider);
    }
	
	// Update is called once per frame
	void Update () {

        //if the bullet went off screen, delete it
        Vector3 originPosition = Camera.main.WorldToViewportPoint(transform.position);

        if (originPosition.x > 1 || originPosition.x < 0 || originPosition.y > 1 || originPosition.y < 0)
            Destroy(gameObject, 0.2f);
	}

    void OnCollisionEnter2D(Collision2D collision) {

        Destroy(gameObject);

        if (collision.gameObject.tag != "Player")
            return;

        //collision with player, damage the player
        HealthManager healthManager = collision.gameObject.GetComponent<HealthManager>() as HealthManager;

        if (healthManager == null)
            return;

        healthManager.decreaseHealth(property.damage);
    }
}
