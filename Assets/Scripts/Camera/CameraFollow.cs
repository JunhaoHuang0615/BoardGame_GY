
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class CameraFollow : MonoBehaviour
{
    private Func<Vector3> GetCameraPosition;
    public float edgeSize = 30f;
    private GameManager gm;
    public static CameraFollow instance;
    private Vector3 recordCameraPosition;
    private void Awake()
    {
        instance = this;
    }

    private void Start()
    {
        ResetCameraPosition();
        gm = FindObjectOfType<GameManager>();
    }
    private void Update()
    {
        
        UpdateCameraPosition();
        EdgeMoveMent();
    }
    public void SetCameraPosition(Func<Vector3> SetCameraPosition)
    {
        this.GetCameraPosition = SetCameraPosition;
    }

    public void UpdateCameraPosition()
    {
        Vector3 tarPosition = GetCameraPosition();
        tarPosition.z = transform.position.z; //为了让目标位置与相机在同一高度

        //当前相机位置与目标位置的方向
        Vector3 cameraMoveDirection = (tarPosition - transform.position).normalized;
        float distance = Vector3.Distance(tarPosition,transform.position);
        float cameraMoveSpeed = 2f; //平滑参数

        //为了避免在低帧情况下出现对同一目标反复跟随的情况
        if (distance > 0)
        {
            Vector3 newCamerPosition = transform.position + cameraMoveDirection * distance * cameraMoveSpeed * Time.deltaTime;
            float distanceAfterMoving = Vector3.Distance(newCamerPosition,tarPosition);

            if(distanceAfterMoving > distance)
            {
                newCamerPosition = tarPosition;
            }

            transform.position = newCamerPosition;
        }
        LimitEdgePosition();

    }
    public void ResetCameraPosition()
    {
        SetCameraPosition(()=> new Vector3(0,0,0));
    }
    public void SetCameraPosition( Vector3 targetPos)
    {
        SetCameraPosition(() => targetPos);
    }
    public void EdgeMoveMent()
    {
        float moveamout = 100f;
        Vector3 currentPosition = transform.position;
        //向右移动
        if(Input.mousePosition.x > Screen.width - edgeSize)
        {
            currentPosition.x += moveamout * Time.deltaTime;
        }
        //向左移动
        if (Input.mousePosition.x < edgeSize)
        {
            currentPosition.x -= moveamout * Time.deltaTime;
        }
        //向上
        if (Input.mousePosition.y > Screen.height - edgeSize)
        {
            currentPosition.y += moveamout * Time.deltaTime;
        }
        //向下
        if (Input.mousePosition.y < edgeSize)
        {
            currentPosition.y -= moveamout * Time.deltaTime;
        }


        SetCameraPosition(currentPosition);
    }

    public void LimitEdgePosition()
    {
        float y = Mathf.Clamp(transform.position.y,gm.downEdgeTile.transform.position.y,gm.upEdgeTile.transform.position.y);
        float x = Mathf.Clamp(transform.position.x, gm.leftEdgeTile.transform.position.x, gm.rightEdgeTile.transform.position.x);
        transform.position = new Vector3(x,y,transform.position.z);
    }
    public void RecordCameraPosition()
    {
        this.recordCameraPosition = transform.position;
    }

    public void ReturnCameraPosition()
    {
        transform.position = this.recordCameraPosition;
    }
}
