using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;



public class FloatText : MonoBehaviour
{
    public float speed = 0.001f;
    public float LifeTime = 2f;
    public Transform TP;
    Transform Textposition;
    //获取摄像机位置
    Transform cam;
    public GameObject FloatTextPrefab;
    public GameObject ExpTextPrefab;
    void LateUpdate()
    {
        if(Textposition!=null)
        {
            Textposition.forward = cam.forward;
            Textposition.position += Vector3.up * Time.deltaTime *speed;
        }
        
        
    }
    void OnEnable()
    {
        cam = Camera.main.transform;    
    }
    public void GenerateDamage(float damage)
    {   
        foreach (Canvas canvas in FindObjectsOfType<Canvas>())
        {
            if(canvas.renderMode == RenderMode.WorldSpace)
            {
                Textposition = Instantiate(FloatTextPrefab,canvas.transform).transform;
            }
        }
        if(Textposition!=null)
        {
        Textposition.position = TP.position;
        Textposition.GetComponent<Text>().text = "-"+damage.ToString();
        } 
    }
    public void GenerateExp(int Exp)
    {   
        foreach (Canvas canvas in FindObjectsOfType<Canvas>())
        {
            if(canvas.renderMode == RenderMode.WorldSpace)
            {
                Textposition = Instantiate(ExpTextPrefab,canvas.transform).transform;
            }
        }
        if(Textposition!=null)
        {
            Textposition.position = TP.position;
            Textposition.GetComponent<Text>().text = "+"+Exp.ToString();
        } 
    }
    public void LevelUp()
    {
        foreach (Canvas canvas in FindObjectsOfType<Canvas>())
        {
            if(canvas.renderMode == RenderMode.WorldSpace)
            {
                Textposition = Instantiate(ExpTextPrefab,canvas.transform).transform;
            }
        }
        if(Textposition!=null)
        {
            Textposition.position = TP.position;
            Textposition.GetComponent<Text>().text = "LevelUp";
        } 
    }
    public void DestroyGameobject()
    {
        if(Textposition.gameObject != null)
            Destroy(Textposition.gameObject,LifeTime);
    }
}
