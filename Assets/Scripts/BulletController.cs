using UnityEngine;
using System.Collections;
using System.Collections.Generic;

//handles all the behaviour for a given type of bullet
//i.e movement
[RequireComponent(typeof(BulletProperties))]
public class BulletController : MonoBehaviour {

    //reference to teamManager to get colliders for players to ignore collision with teamamates of shooter
    public static TeamManager teamManager;
    
    //id of the team that fired the bullet
    [System.NonSerialized]
    public TeamProperties.Teams idTeamThatFiredBullet;

    //property of the bullet this behaviour refers to
    public BulletProperties property;
    public Rigidbody2D rigidBody;

    //bullets collider cached
    [SerializeField]private new BoxCollider2D collider;

	// Use this for initialization
	void Start () {

        if (rigidBody == null)
            Debug.Log("rigidBody in BulletController is null");

        if (property == null)
            Debug.Log("Bullet property is null");

        if(collider == null)
            Debug.Log("BulletController has no collider reference");

        if (teamManager == null)
            teamManager = GameObject.FindWithTag("TeamManager").GetComponent<TeamManager>() as TeamManager;
    }


    //when gun is fired it will create a bullet
    //this function will use the given parameters to determine the bullets movement
    //gunAngle is the angle of the gun IN RADIANS
    //gunAimDeviation is how much a bullet should strayf rom the intended gunAngle IN DEGREES
    //shooter is a referencce to the player who shot the bullet, used dto setup collision masking
    public void fire(Vector3 startingPosition, float gunAngle, float gunAimDeviation, PlayerController shooter) {

        transform.position = startingPosition;

        float actualFireAngle = Mathf.Rad2Deg * gunAngle + Random.Range(-gunAimDeviation, gunAimDeviation);

        //make bullet point in the same direction as the gun
        transform.rotation = Quaternion.Euler(0, 0, actualFireAngle);

        rigidBody.velocity = Quaternion.Euler(0, 0, actualFireAngle) * Vector2.right * property.speed;

        setupCollisionsToIgnore(shooter);
    }

    void setupCollisionsToIgnore(PlayerController shooter) {

        Physics2D.IgnoreCollision(collider, shooter.getCollider());

        if (teamManager == null)
            teamManager = GameObject.FindWithTag("TeamManager").GetComponent<TeamManager>() as TeamManager;

        List<BoxCollider2D> teammateColliders = teamManager.collidersForEachTeam[shooter.getTeam()];

        foreach (BoxCollider2D boxCollider in teammateColliders)
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
