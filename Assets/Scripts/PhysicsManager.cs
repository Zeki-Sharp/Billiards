using UnityEngine;

public class PhysicsManager : MonoBehaviour
{
    public static PhysicsManager Instance;
    
    [Header("碰撞设置")]
    public float whiteBallMinSpeed = 3f; // 白球最小保持速度
    public float whiteBallBounceForce = 8f; // 白球反弹力度
    public float redBallBounceForce = 5f; // 红球反弹力度
    
    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }
    
    public void HandleBallCollision(BallPhysics ball1, BallPhysics ball2)
    {
        if (ball1 == null || ball2 == null) return;
        
        Debug.Log($"PhysicsManager处理碰撞: 球1速度={ball1.GetSpeed():F2}, 球2速度={ball2.GetSpeed():F2}");
        
        // 暂时不做任何特殊处理，让Unity物理引擎自然处理碰撞
        // 后续可以根据需要添加特殊规则
    }
    
    void ApplyWhiteBallBounceRule(BallPhysics whiteBall, BallPhysics redBall, Vector2 collisionDirection)
    {
        // 白球总是获得反向的反弹力
        Vector2 whiteBallForce = -collisionDirection * whiteBallBounceForce;
        whiteBall.ApplyForce(whiteBallForce);
        
        // 红球获得正向的力
        Vector2 redBallForce = collisionDirection * redBallBounceForce;
        redBall.ApplyForce(redBallForce);
        
        // 确保白球保持最小速度
        EnsureWhiteBallMinSpeed(whiteBall);
        
        Debug.Log($"应用反弹规则: 白球获得力={whiteBallForce}, 红球获得力={redBallForce}");
    }
    
    void EnsureWhiteBallMinSpeed(BallPhysics whiteBall)
    {
        float currentSpeed = whiteBall.GetSpeed();
        
        if (currentSpeed < whiteBallMinSpeed)
        {
            Vector2 currentVelocity = whiteBall.GetVelocity();
            Vector2 direction = currentVelocity.normalized;
            
            // 如果速度太小，使用随机方向
            if (direction.magnitude < 0.1f)
            {
                direction = Random.insideUnitCircle.normalized;
            }
            
            Vector2 minVelocity = direction * whiteBallMinSpeed;
            whiteBall.SetVelocity(minVelocity);
            
            Debug.Log($"确保白球最小速度: {currentSpeed:F2} -> {whiteBallMinSpeed:F2}");
        }
    }
    
    // 公共方法，供外部调用
    public void SetWhiteBallMinSpeed(float speed)
    {
        whiteBallMinSpeed = speed;
    }
    
    public void SetWhiteBallBounceForce(float force)
    {
        whiteBallBounceForce = force;
    }
    
    public void SetRedBallBounceForce(float force)
    {
        redBallBounceForce = force;
    }
}
