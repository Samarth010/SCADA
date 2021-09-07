using System;
using System.Collections.Generic;
using System.Linq;
using System.Media;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace SCADA
{
	/// <summary>
	/// Interaction logic for MainWindow.xaml
	/// </summary>
	/// 

	class Valve : Label
	{
		private int currentTankPressure;
		private int[] stageLimits;
		private bool inDanger, valveOpen;
		private static bool beeping;
		private static SoundPlayer soundPlayer;
		private RotateTransform gaugeTransformer;
		private Label valveStatus;
		private Button valveBtn;
		private static List<Valve> registerInDanger = new List<Valve>();


		public bool Beeping { get; set; }

		public Button ValveBtn
		{

			set
			{
				valveBtn = value;
				valveBtn.Click += (sender, e) =>
				{
					ValveOpen = !ValveOpen;
				};	
			}
		}

		public Label ValveStatus
		{
			set
			{
				valveStatus = value;
				valveStatus.Content = ValveOpen ? "Open" : "Close";

			}
		}

		public bool InDanger
		{
			get
			{
				return inDanger;
			}
			set
			{
				this.inDanger = value;
				if (value && ValveOpen)
				{
					if (registerInDanger.IndexOf(this) == -1) registerInDanger.Add(this);
					Background = Brushes.Red;
					soundPlayer = BlowSound();	
				}
				else
				{
					if (registerInDanger.IndexOf(this) > -1) registerInDanger.Remove(this);
					//registerInDanger.Remove(this);
					Background = Brushes.YellowGreen;
					StopSound();
				}
			}
		}

		public bool ValveOpen
		{
			get { return valveOpen; }
			set
			{
				valveOpen = value;
				if (valveStatus != null) valveStatus.Content = ValveOpen ? "Open" : "Close";
				if (value && (CurrentStage == 0 || CurrentStage == 3)) InDanger = true;
				if (!value) InDanger = false;

			}
		}
		public int MaxCap { get; set; }
		public RotateTransform GaugeTransformer
		{
			get { return gaugeTransformer; }
			set
			{
				gaugeTransformer = value;
				gaugeTransformer.Angle = ((180 * currentTankPressure) / MaxCap);
			}
		}

		public Valve()
		{
			//MaxCap = 50;
			StageLimits = new int[] { 5, 50, 90, 100 };
			CurrentTankPressure = 0;
			CurrentStage = 0;
			Content = String.Format("{0:d3} PSI", CurrentTankPressure);
			beeping = false;
			ValveOpen = false;
		}

		public int CurrentTankPressure
		{
			get
			{
				return currentTankPressure;
			}
			set
			{
				if (value <= MaxCap)
				{
					currentTankPressure = value;
					Content = String.Format("{0:d3} PSI", currentTankPressure);
					if (GaugeTransformer != null) GaugeTransformer.Angle = ((180 * value) / MaxCap);
					if (currentTankPressure < StageLimits[0]) { CurrentStage = 0; InDanger = true; }
					else if (currentTankPressure < StageLimits[1]) { CurrentStage = 1; InDanger = false; }
					else if (currentTankPressure < StageLimits[2]) { CurrentStage = 2; InDanger = false; }
					else if (currentTankPressure < StageLimits[3])
					{
						CurrentStage = 3;
						InDanger = true;
					}
				}
				else valveOpen = false;
			}
		}
		public int[] StageLimits
		{
			get { return stageLimits; }
			set
			{
				MaxCap = value.Max();
				stageLimits = value;
			}
		}
		public int CurrentStage { set; get; }

		public static SoundPlayer BlowSound()
		{
			SoundPlayer alarmSound = new SoundPlayer();

			alarmSound.SoundLocation = @"../../Alert.wav";

			alarmSound.Load();
			if (!beeping)
			{
				beeping = true;
				var cancellationTokenSource = new CancellationTokenSource();
				var alarmProcess = Task.Factory.StartNew(() => { alarmSound.PlayLooping(); }, cancellationTokenSource.Token);
			}
			return alarmSound;
		}
		public static void StopSound()
		{
			if (soundPlayer != null && registerInDanger.Count == 0)
			{
				soundPlayer.Stop();
				beeping = false;
			}
		}
	}
	public partial class MainWindow : Window
	{
		List<Valve> valveList;
		public bool IsOxygenGenOn;
		public MainWindow()
		{
			InitializeComponent();
			IsOxygenGenOn = false;
			valveList = new List<Valve>();
			//Creating list of tanks

			lblTank1Level.ValveStatus = lbl_statustank1;
			lblTank1Level.ValveBtn = btn_tank1;

			lblTank2Level.ValveStatus = lbl_statustank2;
			lblTank2Level.ValveBtn = btn_tank2;

			lblTank3Level.ValveStatus = lbl_statustank3;
			lblTank3Level.ValveBtn = btn_tank3;

			lblTank4Level.ValveStatus = lbl_statustank4;
			lblTank4Level.ValveBtn = btn_tank4;


			lblTank5Level.ValveStatus = lbl_statustank5;
			lblTank5Level.ValveBtn = btn_tank5;


			lblTank6Level.ValveStatus = lbl_statustank6;
			lblTank6Level.ValveBtn = btn_tank6;

			valveList.Add(lblTank1Level);
			valveList.Add(lblTank2Level);
			valveList.Add(lblTank3Level);

			valveList.Add(lblTank4Level);
			valveList.Add(lblTank5Level);
			valveList.Add(lblTank6Level);
		}

		private async void GenerateOxygen(int maxPressure, List<Valve> tanksToFill)
		{
			int currentPressure = 0, currentTankIndex = 0;
			Valve currentTank;
			
			
			while (currentPressure < maxPressure)
			{

				if (currentTankIndex == tanksToFill.Count())
				{
					currentTankIndex =0;
				}
				currentTank = tanksToFill[currentTankIndex++];
				if (currentTank.ValveOpen)
				{
					currentTank.CurrentTankPressure++;
					currentPressure++;
					if(currentTank.CurrentTankPressure == 100)
					{
						await Task.Delay(2000);
						currentTank.ValveOpen = false;
						currentTank.CurrentTankPressure = 0;

					}
					lblWatch.Content = String.Format("Status: \nCurrent Tank #: {0}; \nCurrent Tank Stage: {1}", currentTankIndex.ToString(), currentTank.CurrentStage);
				}
				await Task.Delay(100);
			}
			
		}


		private void Button_Click(object sender, RoutedEventArgs e)
		{
			
			//MainSystem win = new MainSystem();
			//win.Show();
			//this.Close();
		}

		private void btn_maintank_Click(object sender, RoutedEventArgs e)
		{
			if (!IsOxygenGenOn)
			{
				lbl_statusmaintank.Content = "Open";
				int generationLimit = 1000;
				IsOxygenGenOn = true;
				GenerateOxygen(generationLimit, valveList);
			}
			else
			{
				lbl_statusmaintank.Content = "Closed";
				IsOxygenGenOn = false;
				int generationLimit = 0;
				GenerateOxygen(generationLimit, valveList);
			}

		}

		private void btn_tank1_Click(object sender, RoutedEventArgs e)
		{

		}

		private void menuItemManual_Click(object sender, RoutedEventArgs e)
		{
			string message = "There are 6 tanks in the system each tank has their own valve and guaze" +
			   " to inspect pressure level. As soon as you start the main Oxygen Generation System Tank, it" +
			   " will start filling up all open oxygen storage tanks.Every tank has four" +
			   " pressure stages(i.e. 1st: 5, 2nd: 50, 3rd: 90, 4th: 100) and if any of" +
			   " those tanks has either 1(i.e.lowest pressure) or 4(i.e.highest pressure)" +
			   " pressure stage. system will set to alert and on reaching maximum capacity" +
			   " tank valve will get closed automatically and in some time the pressure will decrease automatically.";
			MessageBox.Show(message, "Manual", MessageBoxButton.OK, MessageBoxImage.Information, MessageBoxResult.OK, MessageBoxOptions.ServiceNotification);
		}

		private void menuItemGoBack_Click(object sender, RoutedEventArgs e)
		{
			Submarine s = new Submarine();
			s.Show();
			this.Close();
		}

		private void menuItemExit_Click(object sender, RoutedEventArgs e)
		{
			this.Close();
		}
	}
}
