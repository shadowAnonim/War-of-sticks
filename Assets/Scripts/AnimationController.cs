using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AnimationController : MonoBehaviour
{
    private Animator animator;
    private FirstPersonAIO hero;

    private void Awake()
    {
        animator = GetComponent<Animator>();
        hero = GameObject.Find("Hero").GetComponent<FirstPersonAIO>();
    }
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if (!hero.dialogMode)
        {
            if (Input.GetAxis("Jump") != 0)
            {
                animator.SetBool("Jump", true);
            }
            else
            {
                animator.SetBool("Jump", false);
                if (Input.GetAxis("Run") != 0 && (Input.GetAxis("Horizontal") != 0 || Input.GetAxis("Vertical") != 0))
                {
                    animator.SetBool("Run", true);
                }
                else
                {
                    animator.SetBool("Run", false);
                    if (Input.GetAxis("Horizontal") != 0 || Input.GetAxis("Vertical") != 0)
                    {
                        animator.SetBool("Walk", true);
                    }
                    else
                    {
                        animator.SetBool("Walk", false);
                    }
                }
            }
        }
        else
        {
            animator.SetBool("Walk", false);
            animator.SetBool("Run", false);
            animator.SetBool("Jump", false);
        }
    }
}
