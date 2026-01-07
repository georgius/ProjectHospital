
This repository contains mod files for [Advanced game changes]() mod from [Project Hospital](https://store.steampowered.com/app/868360/Project_Hospital/) game.

- Data - contains mod static data (assets, database files)
- ModAdvancedGameChanges - contains Visual Studio 2026 solution with sources and dependencies

# Requirements
The mod requires [**Hospital Services DLC**](https://store.steampowered.com/app/1282550/Project_Hospital__Hospital_Services/) to work.
Without [**Hospital Services DLC**](https://store.steampowered.com/app/1282550/Project_Hospital__Hospital_Services/) will be all effects of mod disabled.

# Changes in mod
- completely changed flows of doctors, nurses, lab specialists, janitors, patients, visitors and pedestrians
- implemented non-linear skill leveling or employee leveling
- implemented *Training department*, where employees can get higher skill or employee level without training costs (wages are still paid)
- allowed pedestrians to go to pharmacy to buy drugs
- modified action time calculation

# Options
The several aspects of mod can be controlled directly from game options.
Some options require restart of game after changing.

| Option | Default | Description |
| -- | -- | -- |
| [AGC] Debug output | off | Enables or disabled verbose output of mod to game log file. |
| [AGC] Enable mod changes (requires restart) | on | Enables or disables **ALL changes** introduced by mod. It is useful when player wants to play game without mod modifications. In such case it is not needed to uninstall mod. |
| [AGC] Non-linear skill leveling | on | Enables or disables non-linear skill leveling and employee leveling. See more at [Non-linear employee leveling](#non-linear-employee-leveling). |
| [AGC] Enable pedestrians go to pharmacy | on | Enables or disables pedestrians to go to pharmacy to buy medicine. See more at [Pedestrians going to pharmacy](#pedestrians-going-to-pharmacy). |
| [AGC] Force employee lowest hire level | on | Forces hiring of all employes with lowest level. |
| [AGC] Biochemistry lab employee (requires restart) | on | Forces lab specialists in *Medical laboratories* department to have *Advanced biochemistry* skill. |
| [AGC] Limit clinic doctors level | on | Enables or disables clinic doctors from gaining more level than specified by tweakables. |
| [AGC] Patients go through emergency | on | Enables or disables that all patients must go through emergency department. If turned off, some patients can go directly to specialized clinic offices. |
| [AGC] Night shift staff lunch | on | Enables or disables lunch for night staff shift. |
| [AGC] Same length for staff shifts (requires restart) | on | Enables or disables same length for day and night shift. |

# Non-linear employee leveling
## Doctors
The non-linear employee leveling for doctors is straightforward.

| Doctor's level | Points needed for next level | Default |
| -- | -- | --: |
| Intern		 | AGC_TWEAKABLE_DOCTOR_LEVEL_POINTS_1 | 20 000 |
| Resident		 | AGC_TWEAKABLE_DOCTOR_LEVEL_POINTS_2 | 40 000 |
| Attending	     | AGC_TWEAKABLE_DOCTOR_LEVEL_POINTS_3 | 80 000 |
| Fellow		 | AGC_TWEAKABLE_DOCTOR_LEVEL_POINTS_4 | 160 000 |
| Specialist	 | maximum level | |

## Nurses, lab specialists and janitors
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

### Nurses
The nurse needs `AGC_TWEAKABLE_NURSE_LEVEL_POINTS_1 + AGC_TWEAKABLE_NURSE_LEVEL_POINTS_2` points to
increase employee level from `Nursing intern` to `Registered nurse`.
The value is same as for doctors increasing from `Intern` to `Attending`.
Nurse gains new skill after `AGC_TWEAKABLE_NURSE_LEVEL_POINTS_1` points,
similar as doctor gains new skill at `Resident` level - after `AGC_TWEAKABLE_DOCTOR_LEVEL_POINTS_1`.

| Nurse's level | Points needed for next level | Default |
| -- | -- | --: |
| Nursing intern | AGC_TWEAKABLE_NURSE_LEVEL_POINTS_1 | 20 000 |
| Nursing intern | AGC_TWEAKABLE_NURSE_LEVEL_POINTS_2 | 40 000 |
| Registered nurse | AGC_TWEAKABLE_NURSE_LEVEL_POINTS_3 | 80 000 |
| Registered nurse | AGC_TWEAKABLE_NURSE_LEVEL_POINTS_4 | 160 000 |
| Nurse specialist | maximum level | |

### Lab specialists
The lab specialist needs `AGC_TWEAKABLE_LAB_SPECIALIST_LEVEL_POINTS_1 + AGC_TWEAKABLE_LAB_SPECIALIST_LEVEL_POINTS_2` points to
increase employee level from `Junior scientist` to `Senior scientist`.
The value is same as for doctors increasing from `Intern` to `Attending`.
Lab specialist gains new skill after `AGC_TWEAKABLE_LAB_SPECIALIST_LEVEL_POINTS_1` points,
similar as doctor gains new skill at `Resident` level - after `AGC_TWEAKABLE_DOCTOR_LEVEL_POINTS_1`.

| Lab specialist's level | Points needed for next level | Default |
| -- | -- | --: |
| Junior scientist | AGC_TWEAKABLE_LAB_SPECIALIST_LEVEL_POINTS_1 | 20 000 |
| Junior scientist | AGC_TWEAKABLE_LAB_SPECIALIST_LEVEL_POINTS_2 | 40 000 |
| Senior scientist | AGC_TWEAKABLE_LAB_SPECIALIST_LEVEL_POINTS_3 | 80 000 |
| Senior scientist | AGC_TWEAKABLE_LAB_SPECIALIST_LEVEL_POINTS_4 | 160 000 |
| Master scientist | maximum level | |

### Janitors
The janitor needs `AGC_TWEAKABLE_JANITOR_LEVEL_POINTS_1 + AGC_TWEAKABLE_JANITOR_LEVEL_POINTS_2` points to
increase employee level from `Janitor` to `Senior janitor`.
The value is same as for doctors increasing from `Intern` to `Attending`.
Janitor gains new skill after `AGC_TWEAKABLE_JANITOR_LEVEL_POINTS_1` points,
similar as doctor gains new skill at `Resident` level - after `AGC_TWEAKABLE_DOCTOR_LEVEL_POINTS_1`.

| Janitor's level | Points needed for next level | Default |
| -- | -- | --: |
| Janitor | AGC_TWEAKABLE_JANITOR_LEVEL_POINTS_1 | 20 000 |
| Janitor | AGC_TWEAKABLE_JANITOR_LEVEL_POINTS_2 | 40 000 |
| Senior janitor | AGC_TWEAKABLE_JANITOR_LEVEL_POINTS_3 | 80 000 |
| Senior janitor | AGC_TWEAKABLE_JANITOR_LEVEL_POINTS_4 | 160 000 |
| Master janitor | maximum level | |

# Non-linear skill leveling
The non-linear skill leveling is similar to non-linear employee level.
It is controlled by main tweakable `AGC_TWEAKABLE_SKILL_LEVELS` and several other tweakables.

# Pharmacy behavior
The pharmacy behavior is completely changed. The main change is possibility of pedestrians to go to pharmacy.
There are several tweakables affecting pharmacy for all characters and several affecting pharmacy behavior for pedestrians.

## General pharmacy tweakables
The general pharmacy behavior for all characters is affected by these tweakables:

| Name | Value |
| :-- | :-- |
| Tweakable | AGC_TWEAKABLE_PHARMACY_CUSTOMER_MAXIMUM_WAITING_TIME_MINUTES |
| Type | int |
| Default | 120 |
| Minimum | 1 |
| Maximum | |
| Description | The maximum waiting time in pharmacy in minutes for all characters. If character is waiting longer, the character leaves pharmacy without payment. |

| Name | Value |
| :-- | :-- |
| Tweakable | AGC_TWEAKABLE_PHARMACY_PHARMACIST_QUESTION_MINUTES |
| Type | int |
| Default | 1 |
| Minimum | 1 |
| Maximum | |
| Description | The base time in minutes for pharmacist question. See more [Action time calculation](#action-time-calculation). |

| Name | Value |
| :-- | :-- |
| Tweakable | AGC_TWEAKABLE_PHARMACY_PHARMACIST_SEARCH_DRUG_MINUTES |
| Type | int |
| Default | 5 |
| Minimum | 1 |
| Maximum | |
| Description | The base time in minutes for pharmacist to search drugs in shelfs. See more [Action time calculation](#action-time-calculation). |

| Name | Value |
| :-- | :-- |
| Tweakable | AGC_TWEAKABLE_PHARMACY_PHARMACIST_SEARCH_DRUG_SKILL_POINTS |
| Type | int |
| Default | 3 |
| Minimum | 1 |
| Maximum | |
| Description | The skill points to be added to `DLC_SKILL_LAB_SPECIALIST_SPEC_PHARMACOLOGY` and to employee level. |

## Patients
The patients buy prescribed drugs by doctor, the prices are specified in game files.
If there are another possible treatments for symptoms of diseases not presribed by doctor, patients buy them by prices specified in game files.

## Pedestrians
The pedestrians buy between `1` and `AGC_TWEAKABLE_PEDESTRIAN_PHARMACY_BUY_UNRESTRICTED_ITEMS_MAXIMUM` (randomly) of non-restricted drugs by price randomly between `AGC_TWEAKABLE_PEDESTRIAN_PHARMACY_UNRESTRICTED_ITEMS_PAYMENT_MINIMUM` and `AGC_TWEAKABLE_PEDESTRIAN_PHARMACY_UNRESTRICTED_ITEMS_PAYMENT_MAXIMUM`
and between `AGC_TWEAKABLE_PEDESTRIAN_PHARMACY_BUY_RESTRICTED_ITEMS_MINIMUM` and `AGC_TWEAKABLE_PEDESTRIAN_PHARMACY_BUY_RESTRICTED_ITEMS_MAXIMUM` (randomly)
of prescribed drugs by price randomly between `AGC_TWEAKABLE_PEDESTRIAN_PHARMACY_RESTRICTED_ITEMS_PAYMENT_MINIMUM` and `AGC_TWEAKABLE_PEDESTRIAN_PHARMACY_RESTRICTED_ITEMS_PAYMENT_MAXIMUM`.

### Pedestrian pharmacy tweakables
The pedestrian pharmacy behavior is affected by these tweakables:

| Name | Value |
| :-- | :-- |
| Tweakable | AGC_TWEAKABLE_PEDESTRIAN_COUNT |
| Type | int |
| Default | 15 |
| Minimum | 0 |
| Maximum | |
| Description | The pedestrians' count simulated by game. |

| Name | Value |
| :-- | :-- |
| Tweakable | AGC_TWEAKABLE_PEDESTRIAN_PHARMACY_PROBABILITY_DAY_PERCENT |
| Type | float |
| Default | 50 |
| Minimum | 0 |
| Maximum | 100 |
| Description | The probability in percent that pedestrian will go to pharmacy in day shift. In other case, pedestrian will just walk around hospital. |

| Name | Value |
| :-- | :-- |
| Tweakable | AGC_TWEAKABLE_PEDESTRIAN_PHARMACY_PROBABILITY_NIGHT_PERCENT |
| Type | float |
| Default | 10 |
| Minimum | 0 |
| Maximum | 100 |
| Description | The probability in percent that pedestrian will go to pharmacy in night shift. In other case, pedestrian will not appear. |

| Name | Value |
| :-- | :-- |
| Tweakable | AGC_TWEAKABLE_PEDESTRIAN_PHARMACY_BUY_UNRESTRICTED_ITEMS_MAXIMUM |
| Type | int |
| Default | 5 |
| Minimum | 1 |
| Maximum | |
| Description | The maximum count of non-restricted drugs (drugs without prescription) to be buyed by pedestrian. The minimum is hard coded to 1. |

| Name | Value |
| :-- | :-- |
| Tweakable | AGC_TWEAKABLE_PEDESTRIAN_PHARMACY_BUY_RESTRICTED_ITEMS_MINIMUM |
| Type | int |
| Default | 0 |
| Minimum | 0 |
| Maximum | |
| Description | The minimum count of restricted drugs (drugs with prescription) to be buyed by pedestrian. |

| Name | Value |
| :-- | :-- |
| Tweakable | AGC_TWEAKABLE_PEDESTRIAN_PHARMACY_BUY_RESTRICTED_ITEMS_MAXIMUM |
| Type | int |
| Default | 3 |
| Minimum | AGC_TWEAKABLE_PEDESTRIAN_PHARMACY_BUY_RESTRICTED_ITEMS_MINIMUM |
| Maximum | |
| Description | The maximum count of restricted drugs (drugs with prescription) to be buyed by pedestrian. |

| Name | Value |
| :-- | :-- |
| Tweakable | AGC_TWEAKABLE_PEDESTRIAN_PHARMACY_UNRESTRICTED_ITEMS_PAYMENT_MINIMUM |
| Type | int |
| Default | 10 |
| Minimum | 0 |
| Maximum | |
| Description | The minimum payment for non-restricted drugs (drugs without prescription). |

| Name | Value |
| :-- | :-- |
| Tweakable | AGC_TWEAKABLE_PEDESTRIAN_PHARMACY_UNRESTRICTED_ITEMS_PAYMENT_MAXIMUM |
| Type | int |
| Default | 50 |
| Minimum | AGC_TWEAKABLE_PEDESTRIAN_PHARMACY_UNRESTRICTED_ITEMS_PAYMENT_MINIMUM |
| Maximum | |
| Description | The maximum payment for non-restricted drugs (drugs without prescription). |

| Name | Value |
| :-- | :-- |
| Tweakable | AGC_TWEAKABLE_PEDESTRIAN_PHARMACY_RESTRICTED_ITEMS_PAYMENT_MINIMUM |
| Type | int |
| Default | 60 |
| Minimum | 0 |
| Maximum | |
| Description | The minimum payment for restricted drugs (drugs with prescription). |

| Name | Value |
| :-- | :-- |
| Tweakable | AGC_TWEAKABLE_PEDESTRIAN_PHARMACY_RESTRICTED_ITEMS_PAYMENT_MAXIMUM |
| Type | int |
| Default | 130 |
| Minimum | AGC_TWEAKABLE_PEDESTRIAN_PHARMACY_RESTRICTED_ITEMS_PAYMENT_MINIMUM |
| Maximum | |
| Description | The maximum payment for restricted drugs (drugs with prescription). |

# Action time calculation
Each action in game (e.g. doctors' examinations, doctors' treatments, nurses' actions, 
lab specialists' actions, janitors' actions) has base time of execution - *base\_action\_time*.
Also each action time is related to some *skill*, so *skill\_level* is part of calculation formulas.
Let the $\mathit{skill\\_level} \in \langle 1.0, 5.0 \rangle$ and define *skill\_ratio* as

```math
\mathit{skill\_ratio}=\frac{\mathit{skill\_level} - 1.0}{4.0} + \mathsf{AGC\_TWEAKABLE\_SKILL\_RATIO\_MINIMUM}
```

or with default settings $\mathit{skill\\_ratio} \in \langle 0.2, 1.2 \rangle$

The action time is calculated with following formula

```math
\mathit{action\_time} = \frac{\mathit{base\_action\_time}}{\mathit{skill\_ratio}} \cdot \frac{100.0}{\mathit{efficiency}}
```

Let the $\mathit{satisfaction} \in \langle 0.0, 100.0 \rangle$, define $\mathit{satisfaction\\_factor}$

```math
\mathit{satisfaction\_factor} = \frac{\mathrm{random}(\mathsf{AGC\_TWEAKABLE\_EFFICIENCY\_SATISFACTION\_MINIMUM}, \mathsf{AGC\_TWEAKABLE\_EFFICIENCY\_SATISFACTION\_MAXIMUM})}{100.0}
```

and define $\mathit{efficiency}$ as

```math
\mathit{efficiency} = \mathrm{max}(\mathsf{AGC\_TWEAKABLE\_EFFICIENCY\_MINIMUM}, \mathit{mood} + \mathit{chief} + \mathit{shift} - \mathit{needs})
```

mood ???

Define $\mathit{chief}$ as

```math
\mathit{chief} = \mathrm{random}(\mathsf{AGC\_TWEAKABLE\_EFFICIENCY\_GOOD\_BOSS\_MINIMUM}, \mathsf{AGC\_TWEAKABLE\_EFFICIENCY\_GOOD\_BOSS\_MAXIMUM})
```

```math
\mathit{chief} = \begin{cases}
\mathrm{random}(\mathsf{AGC\_TWEAKABLE\_EFFICIENCY\_GOOD\_BOSS\_MINIMUM}, \mathsf{AGC\_TWEAKABLE\_EFFICIENCY\_GOOD\_BOSS\_MAXIMUM}), & \text{if employee's chief has perk \textbf{Good Boss}} \\
    0, & \text{otherwise}
  \end{cases}
```

```math
\mathit{chief} = \begin{cases} 
\text{if employee's chief has perk \textbf{Good Boss}} & \Rightarrow \mathrm{random} \begin{pmatrix} \mathsf{AGC\_TWEAKABLE\_EFFICIENCY\_GOOD\_BOSS\_MAXIMUM} \\ \mathsf{AGC\_TWEAKABLE\_EFFICIENCY\_GOOD\_BOSS\_MINIMUM} \end{pmatrix} \\
\text{otherwise} & \Rightarrow 0
\end{cases}
```


```math
\mathit{efficiency} = \mathsf{AGC\_TWEAKABLE\_EFFICIENCY\_MINIMUM} + \frac{100.0 - \mathsf{AGC\_TWEAKABLE\_EFFICIENCY\_MINIMUM}}{100.0} \cdot \mathit{satisfaction} \cdot \mathit{satisfaction\_factor}
```

## Action time tweakables

| Name | Value |
| :-- | :-- |
| Tweakable | AGC_TWEAKABLE_SKILL_RATIO_MINIMUM |
| Type | float |
| Default | 0.2 |
| Minimum | 0.001 |
| Maximum | 5.0 |
| Description | The minimum value of $\mathit{skill\\_ratio}$. In default settings, with minimum possible skill level ($\mathit{skill\\_level}=1.0$) the $\mathit{skill\\_ratio}=0.2$, with maximum possible skill level ($\mathit{skill\\_level}=5.0$) the $\mathit{skill\\_ratio}=1.2$. When $\mathit{skill\\_ratio}$ is lower than 1, the action time is longer as base action time, when $\mathit{skill\\_ratio}$ is greater than 1, the action time is shorter than base action time.  |

| Name | Value |
| :-- | :-- |
| Tweakable | AGC_TWEAKABLE_EFFICIENCY_MINIMUM |
| Type | float |
| Default | 20.0 |
| Minimum | 1.0 |
| Maximum | 100.0 |
| Description | The minimum TO DO. |

| Name | Value |
| :-- | :-- |
| Tweakable | AGC_TWEAKABLE_EFFICIENCY_GOOD_BOSS_MINIMUM |
| Type | float |
| Default | 5.0 |
| Minimum | 0.0 |
| Maximum | 100.0 |
| Description | The minimum TO DO. |

| Name | Value |
| :-- | :-- |
| Tweakable | AGC_TWEAKABLE_EFFICIENCY_GOOD_BOSS_MAXIMUM |
| Type | float |
| Default | 10.0 |
| Minimum | AGC_TWEAKABLE_EFFICIENCY_GOOD_BOSS_MINIMUM |
| Maximum | 100.0 |
| Description | The minimum TO DO. |

# Tweakables
The **Advanced game changes** mod has many tweakable values.

TO DO

# Development
The **Advanced game changes** mod is developed in Visual Studio 2026 Community, the project needs *.NET Framework 3.5* to be installed. The .NET Framework 3.5 can be added in *Control Pannel* / *Programs and Features* / *Turn Windows features on or off*. When installing Visual Studio 2026 Community, add *NET Framework 3.5 development tools* component.