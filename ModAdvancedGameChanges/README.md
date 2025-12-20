
This repository contains mod files for [Advanced game changes]() mod from [Project Hospital](https://store.steampowered.com/app/868360/Project_Hospital/) game.

- Data - contains mod static data (assets, database files)
- ModAdvancedGameChanges - contains Visual Studio 2026 solution with sources and dependencies

# Requirements
The mod requires **Hospital services DLC** to work. Without **Hospital services DLC** will be all effects of mod disabled.

# Changes in mod
- completely changed flows of doctors, nurses, lab specialists, janitors, patients, visitors and pedestrians
- implemented non-linear skill leveling or employee leveling
- implemented *Training department*, where employees can get higher skill or employee level without training costs (wages are still paid)
- allowed pedestrians to go to pharmacy to buy medicine

# Options
The several aspects of mod can be controlled directly from game options.
Some options require restart of game after changing.

| Option | Default | Description |
| -- | -- | -- |
| [AGC] Debug output | off | Enables or disabled verbose output of mod to game log file. |
| [AGC] Enable mod changes (requires restart) | on | Enables or disables **ALL changes** introduced by mod. It is useful when player wants to play game without mod modifications. In such case it is not needed to uninstall mod. |
| [AGC] Non-linear skill leveling | on | Enables or disables non-linear skill leveling and employee leveling. See more at [Non-linear employee leveling](#non-linear-employee-leveling). |
| [AGC] Enable pedestrian go to pharmacy | on | Enables or disables pedestrians to go to pharmacy to buy medicine. See more at [Pedestrian go to pharmacy](#pedestrian-go-to-pharmacy). |
| [AGC] Force employee lowest hire level | on | Forces hiring of all employes with lowest level. |
| [AGC] Biochemistry lab employee (requires restart) | on | Forces lab specialists in *Medical laboratories* department to have *Advanced biochemistry* skill. |
| [AGC] Limit clinic doctors level | on | Enables or disables clinic doctors from gaining more level than specified by tweakables. |
| [AGC] Patients go through emergency | on | Enables or disables that all patients must go through emergency department. If turned off, some patients can go directly to specialized clinic offices. |
| [AGC] Night shift staff lunch | on | Enables or disables lunch for night staff shift. |
| [AGC] Same length for staff shifts (requires restart) | on | Enables or disables same length for day and night shift. |

# Tweakables
The **Advanced game changes** mod has many tweakable values.

## Non-linear employee leveling
### Doctors
The non-linear employee leveling for doctors is straightforward.

| Doctor's level | Points needed for next level | Default |
| -- | -- | -- |
| Intern		 | AGC_TWEAKABLE_DOCTOR_LEVEL_POINTS_1 | 20 000 |
| Resident		 | AGC_TWEAKABLE_DOCTOR_LEVEL_POINTS_2 | 40 000 |
| Attending	     | AGC_TWEAKABLE_DOCTOR_LEVEL_POINTS_3 | 80 000 |
| Fellow		 | AGC_TWEAKABLE_DOCTOR_LEVEL_POINTS_4 | 160 000 |
| Specialist	 | maximum level | |

### Nurses, lab specialists and janitors
The nurses, lab specialists and janitors have only 3 employee levels instead of 5 as doctors.
To simulate same behavior as for doctors, there are added 2 virtual employee levels, see following table.
The level must be virtual, because it is not possible to modify user interface with mod.
The game will notify about level change as for doctors, but on virtual level will nurses,
lab specialists and janitors gain new skill - same as doctors on `Resident` and `Fellow` levels.

| Doctor's level | Nurse's level | Lab specialist's level | Janitor's level |
| -- | -- | -- | -- |
| Intern | Nursing intern | Junior scientist | Janitor |
| Resident | Nursing intern | Junior scientist | Janitor |
| Attending | Registered nurse | Senior scientist | Senior janitor |
| Fellow | Registered nurse | Senior scientist | Senior janitor |
| Specialist | Nurse specialist | Master scientist | Master janitor |

#### Nurses
The nurse needs `AGC_TWEAKABLE_NURSE_LEVEL_POINTS_1 + AGC_TWEAKABLE_NURSE_LEVEL_POINTS_2` points to
increase employee level from `Nursing intern` to `Registered nurse`.
The value is same as for doctors increasing from `Intern` to `Attending`.
Nurse gains new skill after `AGC_TWEAKABLE_NURSE_LEVEL_POINTS_1` points,
similar as doctor gains new skill at `Resident` level - after `AGC_TWEAKABLE_DOCTOR_LEVEL_POINTS_1`.

| Nurse's level | Points needed for next level | Default |
| -- | -- | -- |
| Nursing intern | AGC_TWEAKABLE_NURSE_LEVEL_POINTS_1 | 20 000 |
| Nursing intern | AGC_TWEAKABLE_NURSE_LEVEL_POINTS_2 | 40 000 |
| Registered nurse | AGC_TWEAKABLE_NURSE_LEVEL_POINTS_3 | 80 000 |
| Registered nurse | AGC_TWEAKABLE_NURSE_LEVEL_POINTS_4 | 160 000 |
| Nurse specialist | maximum level | |

#### Lab specialists
The lab specialist needs `AGC_TWEAKABLE_LAB_SPECIALIST_LEVEL_POINTS_1 + AGC_TWEAKABLE_LAB_SPECIALIST_LEVEL_POINTS_2` points to
increase employee level from `Junior scientist` to `Senior scientist`.
The value is same as for doctors increasing from `Intern` to `Attending`.
Lab specialist gains new skill after `AGC_TWEAKABLE_LAB_SPECIALIST_LEVEL_POINTS_1` points,
similar as doctor gains new skill at `Resident` level - after `AGC_TWEAKABLE_DOCTOR_LEVEL_POINTS_1`.

| Lab specialist's level | Points needed for next level | Default |
| -- | -- | -- |
| Junior scientist | AGC_TWEAKABLE_LAB_SPECIALIST_LEVEL_POINTS_1 | 20 000 |
| Junior scientist | AGC_TWEAKABLE_LAB_SPECIALIST_LEVEL_POINTS_2 | 40 000 |
| Senior scientist | AGC_TWEAKABLE_LAB_SPECIALIST_LEVEL_POINTS_3 | 80 000 |
| Senior scientist | AGC_TWEAKABLE_LAB_SPECIALIST_LEVEL_POINTS_4 | 160 000 |
| Master scientist | maximum level | |

#### Janitors
The janitor needs `AGC_TWEAKABLE_JANITOR_LEVEL_POINTS_1 + AGC_TWEAKABLE_JANITOR_LEVEL_POINTS_2` points to
increase employee level from `Janitor` to `Senior janitor`.
The value is same as for doctors increasing from `Intern` to `Attending`.
Janitor gains new skill after `AGC_TWEAKABLE_JANITOR_LEVEL_POINTS_1` points,
similar as doctor gains new skill at `Resident` level - after `AGC_TWEAKABLE_DOCTOR_LEVEL_POINTS_1`.

| Janitor's level | Points needed for next level | Default |
| -- | -- | -- |
| Janitor | AGC_TWEAKABLE_JANITOR_LEVEL_POINTS_1 | 20 000 |
| Janitor | AGC_TWEAKABLE_JANITOR_LEVEL_POINTS_2 | 40 000 |
| Senior janitor | AGC_TWEAKABLE_JANITOR_LEVEL_POINTS_3 | 80 000 |
| Senior janitor | AGC_TWEAKABLE_JANITOR_LEVEL_POINTS_4 | 160 000 |
| Master janitor | maximum level | |

## Non-linear skill leveling
The non-linear skill leveling is similar to non-linear employee level.
It is controlled by main tweakable `AGC_TWEAKABLE_SKILL_LEVELS` and several other tweakables.

## Pedestrian go to pharmacy
There are several tweakables affecting pharmacy for all and several affecting pharmacy behavior for pedestrians.

### General pharmacy tweakables
General pharmacy behavior for all is affected by these tweakables

| Tweakable | Default | Type | Minimum | Maximum | Description |
| -- | -- | -- | -- | -- | -- |
| AGC_TWEAKABLE_PHARMACY_CUSTOMER_MAXIMUM_WAITING_TIME_MINUTES | 120 | int | 1 | | The maximum waiting time (in minutes) in pharmacy for character. If character is waiting more, character leaves pharmacy without payment. |
| AGC_TWEAKABLE_PHARMACY_PHARMACIST_QUESTION_MINUTES | 1 | int | 1 | | ??? |
| AGC_TWEAKABLE_PHARMACY_PHARMACIST_SEARCH_DRUG_MINUTES | 1 | int | 1 | | ??? |
| AGC_TWEAKABLE_PHARMACY_PHARMACIST_SEARCH_DRUG_SKILL_POINTS | 3 | int | 1 | | ??? |
| AGC_TWEAKABLE_PHARMACY_NON_RESTRICTED_DRUGS_PAYMENT_MINIMUM | 10 | int | 0 | | ??? |
| AGC_TWEAKABLE_PHARMACY_NON_RESTRICTED_DRUGS_PAYMENT_MAXIMUM | 50 | int | AGC_TWEAKABLE_PHARMACY_NON_RESTRICTED_DRUGS_PAYMENT_MINIMUM + 1 | | ??? |


# Development
The **Advanced game changes** mod is developed in Visual Studio 2026 Community, the project needs *.NET Framework 3.5* to be installed. The .NET Framework 3.5 can be added in *Control Pannel* / *Programs and Features* / *Turn Windows features on or off*. When installing Visual Studio 2026 Community, add *NET Framework 3.5 development tools* component.