using Core;
using System;
using System.IO;
using Game;
using UnityEngine;

public static class PracticeManager
{

	public static void Initialize()
	{
		PracticeManager.MessageProvider = (PracticeMessageProvider)ScriptableObject.CreateInstance(typeof(PracticeMessageProvider));
		PracticeManager.StartPosition = new Vector2(189f, -219.5f);
		PracticeManager.EndPosition = new Vector2(138f, -218f);
		PracticeManager.EndType = 4;
		PracticeManager.EndReadable = "left";
		PracticeManager.MessageInQueue = 0;
		PracticeManager.Message = "";
		PracticeManager.LastTick = DateTime.Now.Ticks;
		PracticeManager.ResetValues();
		PracticeManager.ParseSessionFile();
	}

	public static void ResetValues()
	{
		PracticeManager.FrameCount = Int32.MaxValue;
		PracticeManager.FrameCountSession = Int32.MaxValue;
		PracticeManager.FrameCountAll = Int32.MaxValue;
		PracticeManager.ExtraTicks = 0L;
		PracticeManager.ExtraFrames = 0;
		PracticeManager.LagFrames = 0;
		PracticeManager.DroppedFrames = 0;
		PracticeManager.MaxDelta = 0L;
		PracticeManager.Running = false;
		PracticeManager.Countdown = -1;
		if(Characters.Sein) 
		{
			Characters.Sein.Mortality.DamageReciever.IsImmortal = false;
		}
		PracticeManager.ResetStats();
	}

	public static void ResetStats()
	{
		PracticeManager.BashStartFrame = -100;
		PracticeManager.BashFrames = 0;
		PracticeManager.BashCount = 0;
		PracticeManager.ChargeDashStartFrame = -100;
		PracticeManager.StompStartFrame = -100;
		PracticeManager.JumpStartFrame = -100;
		PracticeManager.StompCancelCount = 0;
		PracticeManager.LateCancelCount = 0;
		PracticeManager.JumpCancelCount = 0;
		PracticeManager.EarlyJumps = 0;
		PracticeManager.ChainChargeDashCount = 0;
		PracticeManager.ChargeDashDuration = 0;
		PracticeManager.ChargeDashEfficiency = 0f;
		PracticeManager.StompDuration = 0;
		PracticeManager.JumpDuration = 0;
	}

	public static void WriteFile()
	{
		StreamWriter streamWriter = new StreamWriter("PracticeSession.txt");
		streamWriter.WriteLine("Start: " + PracticeManager.StartPosition.x.ToString() + ", " + PracticeManager.StartPosition.y.ToString());
		streamWriter.WriteLine("End: " + PracticeManager.EndPosition.x.ToString() + ", " + PracticeManager.EndPosition.y.ToString() + ", " + PracticeManager.EndType.ToString());
		streamWriter.WriteLine("Best: " + PracticeManager.FrameCountAll.ToString());
		streamWriter.Flush();
		streamWriter.Close();
	}

	public static void ParseSessionFile() 
	{
		if (!File.Exists("PracticeSession.txt"))
		{
			PracticeManager.WriteFile();
		}
		try
		{
			string[] lines = File.ReadAllLines("PracticeSession.txt");
			string[] start = lines[0].Split(':')[1].Split(',');
			PracticeManager.StartPosition = new Vector2(float.Parse(start[0]), float.Parse(start[1]));
			string[] end = lines[1].Split(':')[1].Split(',');
			PracticeManager.EndPosition = new Vector2(float.Parse(end[0]), float.Parse(end[1]));
			PracticeManager.EndType = int.Parse(end[2]);
			PracticeManager.SetEndReadable();
			string[] best = lines[2].Split(':');
			PracticeManager.FrameCountAll = int.Parse(best[1]);
		}
		catch (Exception)
		{
			PracticeManager.ShowMessage("Error parsing session file");
		}

	}

	public static void Update()
	{
		PracticeManager.UpdateMessages();
		long currentTicks = DateTime.Now.Ticks;
		long tickDelta = currentTicks - PracticeManager.LastTick - 166667L;
		PracticeManager.LastTick = currentTicks;
		if (PracticeManager.Countdown >= 0)
		{
			PracticeManager.Countdown--;
			if (Characters.Sein)
			{
				Characters.Sein.Position = PracticeManager.StartPosition;
			}
			if (PracticeManager.Countdown == 0)
			{
				PracticeManager.Start();
			}

		}
		if (PracticeManager.Running)
		{
			if(PracticeManager.FrameCount == 1)
			{
				foreach (Core.Input.InputButtonProcessor button in Core.Input.Buttons)
					{
						if(button != Core.Input.AnyStart)
						{
							button.IsPressed = false;
							button.WasPressed = false;
						}
					}
			}
			PracticeManager.ExtraTicks -= tickDelta;
			if (PracticeManager.ExtraTicks > 166667L)
			{
				PracticeManager.ExtraFrames += 1;
				PracticeManager.ExtraTicks -= 166667L;
			}
			if (PracticeManager.ExtraTicks < -166667L)
			{
				PracticeManager.LagFrames += 1;
				PracticeManager.ExtraTicks += 166667L;
			}
			if (tickDelta > 166667L)
			{
				PracticeManager.DroppedFrames += (int)(tickDelta / 166667L);
			}
			PracticeManager.MaxDelta = Math.Max(PracticeManager.MaxDelta, tickDelta);
			PracticeManager.FrameCount++;
			PracticeManager.CheckEnd();
		}
		if(MoonInput.GetKey(KeyCode.LeftAlt) || MoonInput.GetKey(KeyCode.RightAlt))
		{
			if(MoonInput.GetKeyDown(KeyCode.Keypad5) || MoonInput.GetKeyDown(KeyCode.Semicolon))
			{
				PracticeManager.SetStart();
			}
			if(MoonInput.GetKeyDown(KeyCode.R))
			{
				PracticeManager.QueueStart();
			}
			if(MoonInput.GetKeyDown(KeyCode.K))
			{
				PracticeManager.Initialize();
			}
			if(MoonInput.GetKeyDown(KeyCode.I))
			{
				PracticeManager.ShowPositionInfo();
			}
			if(MoonInput.GetKeyDown(KeyCode.T))
			{
				PracticeManager.MessageInQueue = 2;
			}
			if(MoonInput.GetKeyDown(KeyCode.E))
			{
				PracticeManager.ShowMessage(PracticeManager.GenerateEfficiencyStats());
			}
			if(MoonInput.GetKeyDown(KeyCode.Keypad1) || MoonInput.GetKeyDown(KeyCode.Comma))
			{
				PracticeManager.SetEnd(1);
			}
			if(MoonInput.GetKeyDown(KeyCode.Keypad2) || MoonInput.GetKeyDown(KeyCode.Period))
			{
				PracticeManager.SetEnd(2);
			}
			if(MoonInput.GetKeyDown(KeyCode.Keypad3) || MoonInput.GetKeyDown(KeyCode.Slash))
			{
				PracticeManager.SetEnd(3);
			}
			if(MoonInput.GetKeyDown(KeyCode.Keypad4) || MoonInput.GetKeyDown(KeyCode.L))
			{
				PracticeManager.SetEnd(4);
			}
			if(MoonInput.GetKeyDown(KeyCode.Keypad6) || MoonInput.GetKeyDown(KeyCode.Quote))
			{
				PracticeManager.SetEnd(6);
			}
			if(MoonInput.GetKeyDown(KeyCode.Keypad7) || MoonInput.GetKeyDown(KeyCode.P))
			{
				PracticeManager.SetEnd(7);
			}
			if(MoonInput.GetKeyDown(KeyCode.Keypad8) || MoonInput.GetKeyDown(KeyCode.LeftBracket))
			{
				PracticeManager.SetEnd(8);
			}
			if(MoonInput.GetKeyDown(KeyCode.Keypad9) || MoonInput.GetKeyDown(KeyCode.RightBracket))
			{
				PracticeManager.SetEnd(9);
			}
		}
	}

	public static void SetStart()
	{
		PracticeManager.ResetValues();
		PracticeManager.StartPosition = new Vector2(Characters.Sein.Position.x, Characters.Sein.Position.y);
		int slot = SaveSlotsManager.CurrentSlotIndex;
		SaveSlotsManager.CurrentSlotIndex = 49;
		SaveSlotsManager.BackupIndex = -1;
		GameController.Instance.CreateCheckpoint();
		GameController.Instance.SaveGameController.PerformSave();
		SaveSlotsManager.CurrentSlotIndex = slot;
		PracticeManager.WriteFile();
		PracticeManager.ShowMessage("Start set: " + PracticeManager.StartPosition.ToString(), 5f);
	}

	public static void SetEnd(int type)
	{
		PracticeManager.ResetValues();
		PracticeManager.EndPosition = new Vector2(Characters.Sein.Position.x, Characters.Sein.Position.y);
		PracticeManager.EndType = type;
		PracticeManager.SetEndReadable();
		PracticeManager.WriteFile();
		PracticeManager.ShowMessage("End set: " + PracticeManager.EndPosition.ToString() + " " + PracticeManager.EndReadable);
	}

	public static void SetEndReadable()
	{
		switch(PracticeManager.EndType)
		{
			case 1:
				PracticeManager.EndReadable = "down-left";
				break;
			case 2:
				PracticeManager.EndReadable = "down";
				break;
			case 3:
				PracticeManager.EndReadable = "down-right";
				break;
			case 4:
				PracticeManager.EndReadable = "left";
				break;
			case 6:
				PracticeManager.EndReadable = "right";
				break;
			case 7:
				PracticeManager.EndReadable = "up-left";
				break;
			case 8:
				PracticeManager.EndReadable = "up";
				break;
			case 9:
				PracticeManager.EndReadable = "up-right";
				break;
			default:
				PracticeManager.EndReadable = "ERROR: end direction unset";
				break;
		}
	}

	public static void QueueStart()
	{
		int currentSlotIndex = SaveSlotsManager.CurrentSlotIndex;
		SaveSlotsManager.CurrentSlotIndex = 49;
		SaveSlotsManager.BackupIndex = -1;
		GameController.Instance.SaveGameController.PerformLoad();
		SaveSlotsManager.CurrentSlotIndex = currentSlotIndex;
		if (Characters.Sein)
		{
			Characters.Sein.Position = PracticeManager.StartPosition;
			Characters.Sein.Mortality.DamageReciever.IsImmortal = true;
			UI.Cameras.Current.MoveToTargetCharacter(0f);
		}
		PracticeManager.ShowMessage("Ready...", 5f);
		PracticeManager.Countdown = 60;
		PracticeManager.ResetStats();
	}

	public static void Start()
	{
		int currentSlotIndex = SaveSlotsManager.CurrentSlotIndex;
		SaveSlotsManager.CurrentSlotIndex = 49;
		SaveSlotsManager.BackupIndex = -1;
		GameController.Instance.SaveGameController.PerformLoad();
		SaveSlotsManager.CurrentSlotIndex = currentSlotIndex;
		if (Characters.Sein)
		{
			Characters.Sein.Position = PracticeManager.StartPosition;
			Characters.Sein.Mortality.DamageReciever.IsImmortal = false;
			UI.Cameras.Current.MoveToTargetCharacter(0f);
		}
		PracticeManager.ShowMessage("$GO$", 1f);
		PracticeManager.Running = true;
		PracticeManager.FrameCount = -1;
		PracticeManager.ExtraTicks = 0L;
		PracticeManager.ExtraFrames = 0;
		PracticeManager.LagFrames = 0;
		PracticeManager.MaxDelta = 0L;
		PracticeManager.DroppedFrames = 0;
	}

	public static void End()
	{
		PracticeManager.Running = false;
		PracticeManager.FrameCount += PracticeManager.LagFrames - PracticeManager.ExtraFrames;
		if(PracticeManager.FrameCount < PracticeManager.FrameCountSession)
		{
			PracticeManager.FrameCountSession = PracticeManager.FrameCount;
		}
		if(PracticeManager.FrameCount < PracticeManager.FrameCountAll)
		{
			PracticeManager.FrameCountAll = PracticeManager.FrameCount;
			PracticeManager.WriteFile();
		}
		PracticeManager.ShowFrameInfo();
	}

	public static void CheckEnd()
	{
		if (!Characters.Sein || Characters.Sein.Position.x < -2000f)
		{
			return;
		}
		switch(PracticeManager.EndType)
		{
			case 1:
				if(Characters.Sein.Position.x < PracticeManager.EndPosition.x && Characters.Sein.Position.y < PracticeManager.EndPosition.y)
				{
					PracticeManager.End();
				}
				break;
			case 2:
				if(Characters.Sein.Position.y < PracticeManager.EndPosition.y)
				{
					PracticeManager.End();
				}
				break;
			case 3:
				if(Characters.Sein.Position.x > PracticeManager.EndPosition.x && Characters.Sein.Position.y < PracticeManager.EndPosition.y)
				{
					PracticeManager.End();
				}
				break;
			case 4:
				if(Characters.Sein.Position.x < PracticeManager.EndPosition.x)
				{
					PracticeManager.End();
				}
				break;
			case 6:
				if(Characters.Sein.Position.x > PracticeManager.EndPosition.x)
				{
					PracticeManager.End();
				}
				break;
			case 7:
				if(Characters.Sein.Position.x < PracticeManager.EndPosition.x && Characters.Sein.Position.y > PracticeManager.EndPosition.y)
				{
					PracticeManager.End();
				}
				break;
			case 8:
				if(Characters.Sein.Position.y > PracticeManager.EndPosition.y)
				{
					PracticeManager.End();
				}
				break;
			case 9:
				if(Characters.Sein.Position.x > PracticeManager.EndPosition.x && Characters.Sein.Position.y > PracticeManager.EndPosition.y)
				{
					PracticeManager.End();
				}
				break;
			default:
				PracticeManager.End();
				break;

		}
	}

	public static void ShowMessage(string message, float time = 5f)
	{
		PracticeManager.Message = message;
		PracticeManager.MessageTime = time;
		PracticeManager.MessageInQueue = 2;
	}

	public static void UpdateMessages()
	{
		if(PracticeManager.MessageInQueue > 0)
		{
			PracticeManager.MessageProvider.SetMessage(PracticeManager.Message);
			UI.Hints.Show(PracticeManager.MessageProvider, HintLayer.GameSaved, PracticeManager.MessageTime);
			PracticeManager.MessageInQueue--;
		}
	}

	public static void ShowPositionInfo()
	{
		UI.SeinUI.ShowUI = true;
		SeinUI.DebugHideUI = false;
		PracticeManager.ShowMessage("Start Position: " + PracticeManager.StartPosition.ToString() +
									"\nEnd Position: " + PracticeManager.EndPosition.ToString() + " " + PracticeManager.EndType.ToString() +
									"\nCurrent Position: " + new Vector2(Characters.Sein.Position.x, Characters.Sein.Position.y).ToString());
	}

	public static void ShowFrameInfo()
	{
		UI.SeinUI.ShowUI = true;
		SeinUI.DebugHideUI = false;
		PracticeManager.ShowMessage("Frames: " + PracticeManager.FrameCount.ToString() +
									"    Session Best: " + PracticeManager.FrameCountSession.ToString() +
									"    Overall: " + PracticeManager.FrameCountAll.ToString() + 
									"\nExtra: " + PracticeManager.ExtraFrames.ToString() +
									"    Lag: " + PracticeManager.LagFrames.ToString() +
									"    Dropped: " + PracticeManager.DroppedFrames.ToString() +
									"    Max Delta: " + (((float)PracticeManager.MaxDelta + 166667L) / 10000f).ToString() + "ms");

	}

	public static void OnBashStart()
	{
		PracticeManager.BashStartFrame = PracticeManager.FrameCount;
	}

	public static void OnBashFinished()
	{
		if(PracticeManager.Running)
		{
			PracticeManager.BashCount += 1;
			PracticeManager.BashFrames += PracticeManager.FrameCount - PracticeManager.BashStartFrame;
		}
	}

	public static void OnChargeDashStart()
	{
		PracticeManager.ChargeDashStartFrame = PracticeManager.FrameCount;
		if(PracticeManager.Running)
		{
			int jumpLength = PracticeManager.FrameCount - PracticeManager.JumpStartFrame;
			PracticeManager.JumpStartFrame = -100;
			if(jumpLength < 24)
			{
				PracticeManager.ChainChargeDashCount += 1;
				PracticeManager.JumpDuration += jumpLength;
			}
		}
	}

	public static void OnStomp()
	{
		if(PracticeManager.Running)
		{
			int cdashLength = PracticeManager.FrameCount - PracticeManager.ChargeDashStartFrame;
			PracticeManager.ChargeDashStartFrame = -100;
			if(cdashLength < 24)
			{
				PracticeManager.StompStartFrame = PracticeManager.FrameCount;
				if(cdashLength <= 12)
				{
					PracticeManager.StompCancelCount += 1;
					PracticeManager.ChargeDashDuration += cdashLength;
					PracticeManager.ChargeDashEfficiency += PracticeManager.ChargeDashEfficiencyTable[cdashLength-1];
				}
				else
				{
					PracticeManager.LateCancelCount += 1;
				}
			}
		}
	}

	public static void OnJump()
	{
		if(PracticeManager.Running)
		{
			int stompLength = PracticeManager.FrameCount - PracticeManager.StompStartFrame;
			PracticeManager.StompStartFrame = -100;
			if(stompLength < 24)
			{
				if(stompLength == 1)
				{
					PracticeManager.EarlyJumps += 1;
				}
				else
				{
					PracticeManager.JumpStartFrame = PracticeManager.FrameCount;
					PracticeManager.JumpCancelCount += 1;
					PracticeManager.StompDuration += stompLength;
				}
			}
		}
	}

	public static string GenerateEfficiencyStats()
	{
		string stats = "Bashes: " + PracticeManager.BashCount.ToString() +
					   "    Excess: " + (PracticeManager.BashFrames - PracticeManager.BashCount * 20).ToString();
		if(PracticeManager.BashFrames > 0)
		{
			stats += "    Efficiency: " + ((PracticeManager.BashCount * 2000) / PracticeManager.BashFrames).ToString() + "%";
		}

		stats += "\nStomp Cancels: " + PracticeManager.StompCancelCount.ToString();
		stats += "    Late: " + PracticeManager.LateCancelCount.ToString();
		if(PracticeManager.StompCancelCount > 0)
		{
			stats += "    Average: " + (((float)(100 * PracticeManager.ChargeDashDuration / PracticeManager.StompCancelCount)) / 100f).ToString() +
					 "    Efficiency: " + ((int)(100 * PracticeManager.ChargeDashEfficiency) / PracticeManager.StompCancelCount).ToString() + "%";
		}

		stats += "\nJump Cancels: " + PracticeManager.JumpCancelCount.ToString() + 
				 "    Early: " + PracticeManager.EarlyJumps.ToString() + 
				 "    Excess: " + (PracticeManager.StompDuration - PracticeManager.JumpCancelCount * 2).ToString();
		if(PracticeManager.StompDuration > 0)
		{
			stats += "   Efficiency: " + ((PracticeManager.JumpCancelCount * 200) / PracticeManager.StompDuration).ToString() + "%";
		}

		stats += "\nChain Charge Dashes: " + PracticeManager.ChainChargeDashCount.ToString() +
				 "    Excess: " + (PracticeManager.JumpDuration - PracticeManager.ChainChargeDashCount).ToString();
		if(PracticeManager.JumpDuration > 0)
		{
			stats += "    Efficiency: " + (PracticeManager.ChainChargeDashCount * 100 / PracticeManager.JumpDuration).ToString() + "%";
		}

		return stats;
	}

	public static PracticeMessageProvider MessageProvider;

	public static float MessageTime;

	public static string Message;

	public static int MessageInQueue;

	public static Vector2 StartPosition;

	public static Vector2 EndPosition;

	public static int EndType;

	public static string EndReadable;

	public static int Countdown;

	public static int FrameCount;

	public static int FrameCountSession;

	public static int FrameCountAll;

	public static bool Running;

	public static long LastTick;

	public static long ExtraTicks;

	public static int LagFrames;

	public static int ExtraFrames;

	public static int DroppedFrames;

	public static long MaxDelta;

	public static int BashStartFrame;

	public static int BashFrames;

	public static int BashCount;

	public static int ChargeDashStartFrame;

	public static int StompStartFrame;

	public static int JumpStartFrame;

	public static int StompCancelCount;

	public static int LateCancelCount;

	public static int JumpCancelCount;

	public static int EarlyJumps;

	public static int ChainChargeDashCount;

	public static int ChargeDashDuration;

	public static float ChargeDashEfficiency;

	public static int StompDuration;

	public static int JumpDuration;

	public static float[] ChargeDashEfficiencyTable = new float[]{0.09179272102f, 0.1831833899f, 0.2741123414f, 0.3644428044f, 0.4539572301f, 0.5423609639f, 0.6292794914f, 0.7142611925f, 0.7967745874f, 0.8762101723f, 0.9485584412f, 1f};

}
