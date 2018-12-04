The main idea behind this mod is to time sections by setting a start point with Alt+Numpad5, setting a desired end point, then using Alt+R to run the segment until you're satisfied.

Here is a demo of some features: https://streamable.com/j6r67

Full command list:

Alt+Numpad5: Set start point (current Ori position)
Alt+Numpad1: Set end area (down-left of current Ori position)
Alt+Numpad2: Set end area (down)
Alt+Numpad3: Set end area (down-right)
Alt+Numpad4: Set end area (left)
Alt+Numpad6: Set end area (right)
Alt+Numpad7: Set end area (up-left)
Alt+Numpad8: Set end area (up)
Alt+Numpad9: Set end area (up-right)

Alt+R: Start the run.
Alt+L: Reload PracticeSession.txt
Alt+I: Show current position, start point, end point, and end direction.
Alt+T: Replay last message

Notes:

When Alt+Numpad5 is pressed, the game is saved in slot 50, then the slot is switched back to where it was before. Slot 50 saves will be overwritten without confirmation. The mod will not function correctly if you run while in slot 50, but running in any other slot will enable you to practice segments containing saves.

The full set of actions that gets taken upon pressing Alt+R:
-The save in slot 50 is loaded
-A hint is printed: "Ready..."
-Ori's position is locked to the start position for 1 second, Ori is invulnerable during this time
-The save in slot 50 is loaded again
-Ori's position is set to the start position
-A hint is printed: "GO"
-Frame counting begins
-Any held inputs are forcibly released and repressed on the first frame of counting, thus you can buffer a dash, jump, bash, etc. at the beginning of the segment for more accurate timing. This feature has some quirks, but is functional -- for example, to buffer a cdash, you must press dash then press charge; pressing them together just gives a regular dash.

UI will be toggled on upon reaching the end automatically to display the results. Results include statistics about game performance during the segment:
Frames - how many times the update loop triggered during the segment plus the number of lag frames minus the number of extra frames
Extra - How many extra frames were rendered above 60FPS. With vsync off, these don't give extra speed as physics are time-based, not frame-based.
Lag - How many frames were skipped due to a series of frames slower than 16.6667ms. With vsync off, lag frames usually have no impact on overall speed, though high numbers of them (more than a single-digit of lag frames within 10s) do indicate poor performance.
Dropped - How many frames were dropped during the segment. Dropped frames are counted when updates are occuring more than 33.3333ms apart. Dropped frames cause real time loss.
Max Delta - The longest time between cycles of the update loop. Good performance will have this value somewhere between 16.6667ms and 33.3333ms. Times greater than 50ms indicate that multiple frames are being dropped at once.

Upon setting a start point, end point, or beating the overall best for your start point/end point, PracticeSession.txt will be written. You could use this to share the same start/end points, but note that you'll also need to either send your slot 50 save over, or create a save in slot 50 in the area. Alt+R will set your position after loading, so just having the same skills and being in roughly the same place is good enough here.

Starting a new game will turn on UI and print a hint that indicates that this mod is active to prevent invalid runs as a result of the recompiled game.

The base dll for this mod is the nopause dll with menu commands, 50 slots, and a 0.5 bash deadzone.

Modified classes:
GameController
NewGameAction

Added classes:
PracticeManager
PracticeMessageProvider