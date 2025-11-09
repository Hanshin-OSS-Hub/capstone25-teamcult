// 이 파일은 MonoBehaviour가 아닙니다.
// "DoEnemyTurn() 이라는 함수를 가지고 있어야 한다"는 규칙(명령서)입니다.
public interface IEnemyTurn
{
    void DoEnemyTurn();
    
    // (선택사항) 플레이어 설정을 강제할 수도 있습니다.
    // Transform player { get; set; } 
}