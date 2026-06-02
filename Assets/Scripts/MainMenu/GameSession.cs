using UnityEngine;

public static class GameSession
{
    public static Difficulty SelectedDifficulty = Difficulty.Easy;

    // 게임 → 메인메뉴로 돌아올 때 true. 메뉴가 "덮은 상태로 시작 → 걷어내기"를 할지 판단.
    // 첫 실행(앱 부팅)에는 false라 메뉴가 안 덮이고 바로 보인다. (static이라 씬 전환에도 유지)
    public static bool ReturnedFromGame = false;
    public static string tipText = "Tip!";
}
