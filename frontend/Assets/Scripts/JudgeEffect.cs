using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class JudgeEffect : MonoBehaviour
{
    static JudgeEffect instance;
    public static JudgeEffect Instance { get { return instance; } }

    List<ParticleSystem> particles = new List<ParticleSystem>();

    void Awake()
    {
        if (instance == null)
            instance = this;
    }

    // Start is called before the first frame update
    void Start()
    {
        // i = 0 - 3: falling lines 1 - 4
        // i = 4, 5: left rotate, right rotate
        for (int i = 0; i < 6; i++)
        {
            particles.Add(transform.GetChild(i).GetComponent<ParticleSystem>());
        }
    }


    public void Init()
    {
        float speed = GameManager.Instance.sheets[GameManager.Instance.title].bpm * 0.4f;
        float life = GameManager.Instance.sheets[GameManager.Instance.title].bpm * 0.008f;

        foreach (ParticleSystem particle in particles)
        {
            var ps = particle.main;
            ps.startSpeed = new ParticleSystem.MinMaxCurve(speed);
            ps.startLifetime = new ParticleSystem.MinMaxCurve(life);
        }
    }

    public void OnEffect(int line)
    {
        particles[line].Emit(3);
    }
}
