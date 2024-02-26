1) `main.py` fails satur kodu modelim, kas ir atbildīgs par pašvadītā transporta stūrēšanas leņķa noteikšanu.
2) `main_objects.py` fails satur kodu modelim, kas ir atbildīgs par ceļa satiksmes zīmju noteikšanu un klasificēšanu
3) `Road_Sign_Classification.ipynb` fails satur kodu, kas ir atbildīgs par modeļa, kurš spēj atpazīt ceļa satiksmes zīmes, trenēšanu.
4) `Behavioural_Cloning.ipynb` fails satur kodu, kas ir atbildīgs par modeļa, kas nosaka nepieciešamo stūrēšanas leņķi, trenēšanu.
5) `BehaviouralCloning_10.h5` ir uztrenētais modelis, kas tika iegūts no trenēšanas procesa, ko veica `Behavioural_Cloning.ipynb`.
6) `sign_classification.h5` ir modelis, kas ir atbildīgs par ceļa satiksmes zīmju klasifikāciju, un ko ieguva no `Road_Sign_Classification.ipynb`

   
============= Ejot uz `/Assets/Scripts/` mapi ================


- `CarController.cs` satur kodu, kas ir atbildīgs par transporta paātrināšanu, kā arī par transporta proporciju nodošanu citiem failiem.
- `TakeScreenshot.cs` fails satur kodu, kas ir atbildīgs par datu iegūšanu, ar kuru tika trenēts `Behavioural_Cloning`.
- `SignLogic.cs` fails satur kodu, kas ir atbildīgs par mašīnas proporciju mainīšanu, atkarībā no ierobežojošām ceļa satiksmes zīmēm.
- `PredictSteering.cs` fails ir atbildīgs par attēlu nodošanu uz _Python_ serveri, kā arī par datu saņemšanu no tā, ko tālāk nodod nepieciešamajiem failiem. Šis fails ir arī atbildīgs par satiksmes zīmju klasifikācijas indeksa saņemšanu, ne tikai transporta stūrēšanas leņķa iegūšanu.
- `PredictionRequest.cs`, `PredictClient.cs` un `RunAbleThread.cs` ir faili, kas ir atbildīgi par savienojuma izveidi starp _Python_ serveri un _Unity_.


=============== SHORTCUTS ==============

0) Var izmantot pogas `W`, `A`, `S` `D`, lai manuāli vadītu transportu. Spiežot `SPACE` pogu, var likt transportam apstāties.
1) Spiežot `R` pogu uz klaviatūras, sākas ierakstīšanas process, ko dara `TakeScreenshot.cs` fails (SVARĪGI: Nespiest pogu `R`, jo failā `TakeScreenshot.cs` ir norādītas nepieciešamās mapes, kas eksistē tikai uz autora datora, un kas visticamāk nestrādātu uz cita datora, kā arī tas izmanto samērā daudz datora resursu)
2) Spiežot `B` pogu var pāriet uz "Automātisko režīmu", proti, tiek izmantoti visi modeļi, lai noteiktu un klasificētu ceļa satiksmes zīmes, kā arī, lai noteiktu transporta stūrēšanas leņķi. (SVARĪGI: Tas arī izmanto samērā daudz datora resursu)
3) Spiežo `1` pogu, transport momentāli pāriet uz pirmo trasi, proti, uz trasi, kurā tas tika trenēts.
4) Spiežot `2` pogu, transports momentāli pāriet uz pirmo trasi, proti, uz trasi, kuru tas vēl nekad nav redzējis.
5) Lai aizvērtu simulatoru, var spiest `ALT` un `F4` pogas vienlaicīgi.


================ KĀ IESĀKT SIMULATOR =========================
1) Lejupielādēt šo projektu.
2) Iejiet mapē `Built/Final/`
3) Mapē atrodas fails `ZPD.exe`, uz ko ir jāuzspiež divas reizes, lai atvērtos simulators.
4) Atvērt konsoli mapē, kura tika ielādēta un iesākt failu `main.py`.
5) Izdarīt to pašu, bet prekš faila `main_objects.py`
// PIEZĪME: Ja neizdara 4. un 5. soli, tad `B` poga simulatorā neko nedos.
### VAI ARĪ ###
1) Lejupielādēt šo projektu.
2) Lejupielādēt `Unity` versiju `2021.3.18f1`
3) `Unity` aplikācijā uzspiest `Open` un atvērt lejupielādēto projektu.
4) Atverot simulatoru, uzspiest `Play` pogu, kas parasti atrodas aplikācijas augšā.
5) Atvērt konsoli mapē, kura tika ielādēta un iesākt failu `main.py`.
6) Izdarīt to pašu, bet prekš faila `main_objects.py`
