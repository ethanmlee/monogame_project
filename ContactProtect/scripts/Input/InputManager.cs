namespace ContactProtect.Input;

using Microsoft.Xna.Framework.Input;

public static class InputManager
{
    private static KeyboardState _currentKeyState;
    private static KeyboardState _previousKeyState;

    public static void Update()
    {
        _previousKeyState = _currentKeyState;
        _currentKeyState = Keyboard.GetState();
    }
    
    public static KeyboardState GetState()
    {
        return _currentKeyState;
    }

    public static bool IsDown(Keys key)
    {
        return _currentKeyState.IsKeyDown(key);
    }

    public static bool KeyPressed(Keys key)
    {
        return _currentKeyState.IsKeyDown(key) && !_previousKeyState.IsKeyDown(key);
    }
    
    public static bool KeyReleased(Keys key)
    {
        return !_currentKeyState.IsKeyDown(key) && _previousKeyState.IsKeyDown(key);
    }
}