using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(AudioSource))]
public class AudioField : MonoBehaviour
{
    public AudioSource Source
    {
        get
        {
            if (!_source)
                _source = GetComponent<AudioSource>();
            return _source;
        }
    }
    private AudioSource _source;

    public Camera camera;

    [SerializeField]
    private Collider collider;
    [SerializeField]
    private AnimationCurve volumeCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);

    // Update is called once per frame
    void Update()
    {
        if (!collider || !camera)
            return;

        //Max distance should be the size of the collider, plus a little buffer. This will create a spherical max area
        float distance = collider.bounds.extents.magnitude + 1f;
        Vector3 direction = camera.transform.position - collider.bounds.center;
        float cameraDistance = direction.magnitude;

        //We don't need to worry about distance of this now
        direction = direction.normalized;

        //Ignore this if we're outside the max range of the source
        if (cameraDistance <= distance)
        {
            //Go from the outside of the collider, pointing inwards towards the centre
            Ray ray = new Ray(collider.bounds.center + (direction * distance), -direction);
            RaycastHit hit;
            if (collider.Raycast(ray, out hit, distance))
            {
                Debug.DrawLine(ray.origin, hit.point, Color.green, 2f);

                Vector3 centreToPoint = hit.point - collider.bounds.center;

                float t = cameraDistance / centreToPoint.magnitude;
                //It's possible for this to return a value greater than one, depending on the shape of the collider
                t = Mathf.Clamp01(t);
                //Invert the value (louder at centre, quieter at edge)
                t = 1 - t;
                //This will normally smooth it out (making the transition from 0->1 non-linear), depending on your curve
                t = volumeCurve.Evaluate(t);

                //Assign the volume :)
                Source.volume = t;
            }
        }
        else
        {
            //Just do max volume
            Debug.DrawLine(camera.transform.position, collider.bounds.center, Color.red, 2f);
            Source.volume = 1;
        }
    }
}
