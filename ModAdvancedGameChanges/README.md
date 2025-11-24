
This repository contains mod files for [Advanced game changes]() mod from [Project Hospital](https://store.steampowered.com/app/868360/Project_Hospital/) game.

- Data - contains mod static data (assets, database files)
- ModAdvancedGameChanges - contains Visual Studio 2026 solution with sources and dependencies

### Changes in mod
- completely changed flows of doctors, nurses, lab specialists, janitors, patients and visitors
- implemented non-linear skill leveling or employee level
- implemented *Training department* (option *[AGC] Training department (requires restart)*), where employees can get higher skill or employee level without training costs (wages are still paid)
- introduced option *[AGC] Force employee lowest hire level* which forces that employee level on hire is always lowest (level 1, skills are on 0%)
- introduced option *[AGC] Biochemistry lab employee (requires restart)* which forces lab specialists in *Medical laboratories* department to have *Advanced biochemistry* skill
- introduced option *[AGC] Limit clinic doctors level* which forces doctors on clinic to have lower maximum level
- introduced option *[AGC] Patients go through emergency* which forces all patients to go through *Emergency* department
- introduced option *[AGC] Night shift staff lunch* which enables food requirement for night shift employees
- introduced option *[AGC] Same length for staff shifts (requires restart)* which forces to recalculate shifts lengths to be equal (12 hours each shift)

### Development
The **Advanced game changes** mod is developed in Visual Studio 2026 Community, the project needs *.NET Framework 3.5* to be installed. The .NET Framework 3.5 can be added in *Control Pannel* / *Programs and Features* / *Turn Windows features on or off*. When installing Visual Studio 2019 Community, add *NET Framework 3.5 development tools* component.