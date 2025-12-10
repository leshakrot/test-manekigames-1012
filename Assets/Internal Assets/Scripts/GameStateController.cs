namespace Game.Bootstrap
{
    public class GameStateController
    {
        public enum GameState
        {
            Moving,
            Combat,
            Puzzle,
            Victory
        }
        
        private GameState _currentState;
        
        public GameState CurrentState => _currentState;
        
        public void SetState(GameState newState)
        {
            _currentState = newState;
        }
    }
}