using UnityEngine;
using System;
using System.Collections.Generic;
using System.Linq;

public class RifleController : MonoBehaviour
{
    public Action OnHitTarget;

    Transform _muzzle;
    List<Transform> _bullets;
    (bool active, Vector3 direction, float lifetime)[] _bulletStates;

    void Start()
    {
        _muzzle = transform.Find("Muzzle");
        var allChildren = transform.GetComponentsInChildren<Transform>();
        _bullets = allChildren.Where(t => t.name.IndexOf("bullet", StringComparison.OrdinalIgnoreCase) >= 0).ToList();
        _bulletStates = new (bool, Vector3, float)[_bullets.Count];
    }

    public void Fire()
    {
        for(int i = 0; i < _bulletStates.Length; ++ i)
        {
            if(!_bulletStates[i].active)
            {
                var fireSound = GetComponent<AudioSource>();
                if(fireSound != null)
                {
                    fireSound.Play();
                }

                _bulletStates[i].active = true;
                _bulletStates[i].direction = (_muzzle.position - transform.position).normalized;
                _bulletStates[i].lifetime = 1.5f;
                _bullets[i].transform.parent = null;
                _bullets[i].transform.position = _muzzle.position;
                break;
            }
        }
    }

    void Update()
    {
        for (int i = 0; i < _bulletStates.Length; ++i)
        {
            if (_bulletStates[i].active)
            {
                float deltaTime = Time.deltaTime;

                float range = 30.0f * deltaTime;
                RaycastHit hit;
                if (Physics.Raycast(_bullets[i].transform.position, _bulletStates[i].direction, out hit, range))
                {
                    DeactivateBullet(i);
                    
                    if(hit.transform.name.IndexOf("target", StringComparison.OrdinalIgnoreCase) >= 0)
                    {
                        hit.transform.gameObject.SetActive(false);
                        OnHitTarget?.Invoke();
                    }
                    continue;
                }

                _bullets[i].transform.position += _bulletStates[i].direction * range;
                if (_bulletStates[i].lifetime >= deltaTime)
                {
                    _bulletStates[i].lifetime -= deltaTime;
                }
                else
                {
                    DeactivateBullet(i);
                }
            }
        }
    }

    void DeactivateBullet(int index) 
    {
        _bulletStates[index].lifetime = 0.0f;
        _bulletStates[index].active = false;
        _bullets[index].transform.parent = transform;
        _bullets[index].transform.localPosition = Vector3.zero;
    }
}
