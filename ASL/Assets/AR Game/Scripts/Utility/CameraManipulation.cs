/*
 ******************************************************************************
 * CameraManipulation.cs
 * Authors: Kelvin Sung
 * Modified by Bill Pham & Phuc Tran
 ******************************************************************************
*/

using UnityEngine;
using System.Collections;

/*
 * Controls: Manipulations, with <Left-Alt> pressed, LMB: tumble/rotate, RMB: Track (move the camera), and Middle-Scroll zoom
 */
public class CameraManipulation : MonoBehaviour
{
    public Transform LookAt;
    public bool freeCam = false;
    private Vector3 offset = new Vector3(-1, 1, 0);

    private float mMouseX = 0f;
    private float mMouseY = 0f;
    private const float kPixelToDegree = 0.1f;
    private const float kPixelToDistant = 0.05f;

    // Use this for initialization
    void Start()
    {

        Debug.Assert(LookAt != null);
    }

    // Update is called once per frame
    void Update()
    {
        freeCam = GameVariables.isFreeCam;
        // Sets camera to game level
        if (LookAt == null)
        {
            GameObject parent =GameObject.Find("Level(Clone)");

            if(parent!=null)
            LookAt = parent.transform;
        }

        if (freeCam == true)
        {
            // this will change the rotation
            //transform.LookAt(LookAt.transform);
            transform.up = Vector3.up;
            transform.forward = (LookAt.transform.localPosition - transform.localPosition).normalized;

            if (Input.GetKey(KeyCode.LeftAlt) &&
                (Input.GetMouseButtonDown(0) || (Input.GetMouseButtonDown(1))))
            {
                mMouseX = Input.mousePosition.x;
                mMouseY = Input.mousePosition.y;
                // Debug.Log("MouseButtonDown 0: (" + mMouseX + " " + mMouseY);
            }
            else if (Input.GetKey(KeyCode.LeftAlt) &&
                    (Input.GetMouseButton(0) || (Input.GetMouseButton(1))))
            {
                float dx = mMouseX - Input.mousePosition.x;
                float dy = mMouseY - Input.mousePosition.y;

                // annoying bug: 
                //     If MouseClick move AND THEN ALT-key
                //     Encounter jump because mMouseX and mMouseY not initialized

                mMouseX = Input.mousePosition.x;
                mMouseY = Input.mousePosition.y;

                if (Input.GetMouseButton(0)) // Camera Rotation
                {
                    RotateCameraAboutUp(-dx * kPixelToDegree);
                    RotateCameraAboutSide(dy * kPixelToDegree);
                }
                else if (Input.GetMouseButton(1)) // Camera tracking
                {
                    Vector3 delta = dx * kPixelToDistant * transform.right + dy * kPixelToDistant * transform.up;
                    transform.localPosition += delta;
                    LookAt.localPosition += delta;
                }
            }

            if (Input.GetKey(KeyCode.LeftAlt))  // dolly or zooming
            {
                Vector2 d = Input.mouseScrollDelta;
                // move camera position towards LookAt
                Vector3 v = transform.localPosition - LookAt.localPosition;
                float dist = v.magnitude;
                v /= dist;
                float m = dist - d.y;
                transform.localPosition = LookAt.localPosition + m * v;
            }
        }
        else
        {
            transform.localPosition = LookAt.localPosition + offset;
            transform.LookAt(LookAt);
        }

    }

    private void RotateCameraAboutUp(float degree)
    {
        Quaternion up = Quaternion.AngleAxis(degree, transform.up);
        RotateCameraPosition(ref up);
    }

    private void RotateCameraAboutSide(float degree)
    {
        Quaternion side = Quaternion.AngleAxis(degree, transform.right);
        RotateCameraPosition(ref side);
    }

    private void RotateCameraPosition(ref Quaternion q)
    {
        Matrix4x4 r = Matrix4x4.TRS(Vector3.zero, q, Vector3.one);
        Matrix4x4 invP = Matrix4x4.TRS(-LookAt.localPosition, Quaternion.identity, Vector3.one);
        Matrix4x4 m = invP.inverse * r * invP;

        Vector3 newCameraPos = m.MultiplyPoint(transform.localPosition);
        if (Mathf.Abs(Vector3.Dot(newCameraPos.normalized, Vector3.up)) < 0.985)
        {
            transform.localPosition = newCameraPos;

            // First way:
                    // transform.LookAt(LookAt);
            // Second way:
                // Vector3 v = (LookAt.localPosition - transform.localPosition).normalized;
                // transform.localRotation = Quaternion.LookRotation(v, Vector3.up);
            // Third way: do everything ourselve!
                Vector3 v = (LookAt.localPosition - transform.localPosition).normalized;
                Vector3 w = Vector3.Cross(v, transform.up).normalized;
                Vector3 u = Vector3.Cross(w, v).normalized;
                // INTERESTING: 
                //    chaning the following directions must be done in specific sequence!
                //    E.g., NONE of the following order works: 
                //          Forward, Up, Right 
                //          Forward, Right, Up 
                //          Right, Forward, Up 
                //          Up, Forward, Right 
                //
                //   Forward-Vector MUST BE set LAST!!: both of the following works!
                //          Right, Up, Forward
                //          Up, Right, Forward
                transform.up = u;
                transform.right = w;
                transform.forward = v;
        }
    }

    public void SetLookAtPos(Vector3 p)
    {
        LookAt.localPosition = p;
    }
}
