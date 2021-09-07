# SCADA
# This ia a project for SCADA Mimic System to monitor 6 valves and a series of 6 pressure gauges with alarm which are connected to 6 oxygen tanks
# In this System there are 6 tanks each as their own valve and gauze to inspect pressure level. As soon as you start the main Oxygen Generation System Tank,
# it will start filling up all open oxygen storage tanks. Every tank has four pressure stages (i.e. 1st: 5, 2nd: 50, 3rd: 90, 4th: 100) 
# and if any of those tanks has either 1(i.e. Lowest pressure) or 4(i.e. Highest pressure) pressure stage. system will set to alert and on reaching maximum capacity 
# tank valve will get closed automatically and in some time the pressure will decrease automatically.

