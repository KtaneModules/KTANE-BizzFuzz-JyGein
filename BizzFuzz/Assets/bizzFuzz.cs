using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using UnityEngine;
using KModkit;
using Rnd = UnityEngine.Random;

public class bizzFuzz : MonoBehaviour {

    public KMBombInfo Bomb;
    public KMAudio Audio;
    
    public KMSelectable[] buttons;
    public TextMesh displayText;
    
    static int ModuleIdCounter = 1;
    int ModuleId;
    private bool ModuleSolved;

    List<string> solution = new List<string>();

    void Awake () {
        ModuleId = ModuleIdCounter++;
        GetComponent<KMBombModule>().OnActivate += Activate;
        /*
        foreach (KMSelectable object in keypad) {
            object.OnInteract += delegate () { keypadPress(object); return false; };
        }
        */

        //button.OnInteract += delegate () { buttonPress(); return false; };

    }

    void OnDestroy () { //Shit you need to do when the bomb ends
      
    }

    void Activate () { //Shit that should happen when the bomb arrives (factory)/Lights turn on

    }

    void Start () { //Shit
        ////////initilizing variables
        //display variables
        int firstEight = Rnd.Range(10000000, 99999999);
        int lastDigit = Rnd.Range(1, 9);
        //phrases table variables
        string[] phrases = { "Fizz", "Buzz", "Fuzz", "Bizz", "Ziff", "Zubb", "Fubb", "Buff", "Zizz" };
        int[] primes = { 2, 3, 5, 7, 11, 13, 17, 19, 23 };
        //other calculation variables
        int eightTotal = 0;
        int total = 0;
        bool[] valid = new bool[9];
        int lastSerial = Bomb.GetSerialNumberNumbers().Last();
        string newNumber = "";
        List<string> validPhrases = new List<string>();
        string finalNumber = "";
        
        //5x loop to generate solutions until it finds a solution that uses a phrase
        for (int f=0; f<5; f++) {
            //Updating Display
            firstEight = Rnd.Range(10000000, 99999999);
            lastDigit = Rnd.Range(1, 9);
            displayText.text = firstEight.ToString() + lastDigit.ToString();

            ////////Finding Solution
            //first/second step
            eightTotal = 0;
            for (int i = 0; i < 8; i++) {
                eightTotal += int.Parse(firstEight.ToString()[i].ToString());
            }
            total = eightTotal * lastDigit % 512;

            valid = new bool[9];
            for (int i = 8; i >= 0; i--) {
                if (total >= Math.Pow(2, i)) {
                    total -= (int)Math.Pow(2, i);
                    valid[i] = true;
                }
            }
            valid = valid.Reverse().ToArray();
            newNumber = "";
            for (int i = 8; i >= 0; i--) {
                newNumber += valid[i] ? lastSerial : 0;
            }
            valid = valid.Reverse().ToArray();

            validPhrases = new List<string>();
            for (int i = 0; i < 9; i++) {
                if (valid[i])
                {
                    validPhrases.Add(phrases[i]);
                }
            }

            finalNumber = "";
            for (int i = 0; i < 9; i++) {
                finalNumber += ((int.Parse("" + newNumber[i]) + int.Parse("" + displayText.text[i])) % 10).ToString();
            }

            //third step
            solution = new List<string>();
            for (int i = 0; i < 9; i++) {
                if (valid[i] && (int.Parse(finalNumber) % primes[i]) == 0) {
                    solution.Add(phrases[i]);
                }
            }

            if(solution.Count != 0) {
                break;
            }
        }
        //Debug.LogFormat("[BizzFuzz #{0}]", ModuleId);
        //Logging the solution

        Debug.LogFormat("[BizzFuzz #{0}] The displayed number is {1}.", ModuleId, displayText.text);

        Debug.LogFormat("[BizzFuzz #{0}] The Valid Phrases are {1}.", ModuleId, string.Join(" ", validPhrases.ToArray()));

        Debug.LogFormat("[BizzFuzz #{0}] The Final Number is {1}.", ModuleId, finalNumber);

        Debug.LogFormat("[BizzFuzz #{0}] The Solution is {1}.", ModuleId, solution.Count == 0 ? "Nothing" : string.Join(" ", solution.ToArray()));

        //buttons
        List<string> input = new List<string>();
        for(int i=2; i<11; i++) {
            int dummy = i;
            buttons[i].OnInteract += delegate () { PhrasePress(buttons[dummy], input); return false; };
        }
        buttons[0].OnInteract += delegate () { buttons[0].AddInteractionPunch(); Audio.PlayGameSoundAtTransform(KMSoundOverride.SoundEffect.ButtonPress, buttons[0].transform); Submit(input, solution, firstEight.ToString() + lastDigit.ToString()); return false; };
        buttons[1].OnInteract += delegate () { buttons[1].AddInteractionPunch(); Audio.PlayGameSoundAtTransform(KMSoundOverride.SoundEffect.ButtonPress, buttons[1].transform); Reset(input, firstEight.ToString() + lastDigit.ToString()); return false; };
    }

    //reset button
    void Reset(List<string> input, string number) {
        input.Clear();
        displayText.text = number;
        displayText.fontSize = 300;
    }
    
    //phrase buttons
    void PhrasePress(KMSelectable phrase, List<string> input) {
        phrase.AddInteractionPunch();
        Audio.PlayGameSoundAtTransform(KMSoundOverride.SoundEffect.ButtonPress, phrase.transform);
        if(ModuleSolved) { return; }
        for(int i=0; i<input.Count; i++) {
            if(input[i] == phrase.transform.Find("New Text").GetComponent<TextMesh>().text) { return; }
        }
        input.Add(phrase.transform.Find("New Text").GetComponent<TextMesh>().text);
        if (input.Count == 1) {
            displayText.text = input[0];
        } else {
            displayText.text += input.Last();
        }
        if(input.Count == 3) {
            displayText.fontSize = 260;
        }
        if(input.Count == 4) {
            displayText.fontSize = 210;
        }
        if(input.Count == 5) {
            displayText.fontSize = 170;
        }
        if(input.Count == 6) {
            displayText.fontSize = 150;
        }
        if(input.Count == 7) {
            displayText.fontSize = 130;
        }
        if(input.Count == 8) {
            displayText.fontSize = 120;
        }
        if(input.Count == 9) {
            displayText.fontSize = 110;
        }
    }

    //submit button
    void Submit (List<string> input, List<string> solution, string number) {
        if (ModuleSolved) { return; }
        Debug.LogFormat("[BizzFuzz #{0}] Submitting {1}.", ModuleId, input.Count==0 ? "Nothing" : string.Join(" ", input.ToArray()));
        if (input.Count != solution.Count) {
            Strike();
            Reset(input, number);
            return;
        }
        for (int i = 0; i < input.Count; i++) {
            if (input[i] != solution[i])
            {
                Strike();
                Reset(input, number);
                return;
            }
        }
        Solve();
    }

    void Update () { //Shit that happens at any point after initialization

    }

    void Solve () {
        GetComponent<KMBombModule>().HandlePass();
        Debug.LogFormat("[BizzFuzz #{0}] Correct! Module Solved.", ModuleId);
        ModuleSolved = true;
    }

    void Strike () {
        GetComponent<KMBombModule>().HandleStrike();
        Debug.LogFormat("[BizzFuzz #{0}] Incorrect! Strike Issued.", ModuleId);
    }

#pragma warning disable 414
    private readonly string TwitchHelpMessage = @"Use !{0} 1-9 to press the phrases in reading order. Use !{0} s/r to press the submit or reset button. Commands may be chained like '35s'.";
#pragma warning restore 414

    KMSelectable[] ProcessTwitchCommand (string Command) {
        foreach(char i in Command) {
            string f = i.ToString().ToLower();
            string[] accepted = { "1", "2", "3", "4", "5", "6", "7", "8", "9", "s", "r", " " };
            int counting = 0;
            foreach(string h in accepted) {
                if(h == f) {
                    counting++;
                }
            }
            if(counting == 0) { return null; }
        }
        List<KMSelectable> buttonList = new List<KMSelectable>();
        foreach(char i in Command) {
            string f = i.ToString().ToLower();
            if(f == "s") {
                buttonList.Add(buttons[0]);
            } else if(f == "r") {
                buttonList.Add(buttons[1]);
            } else {
                buttonList.Add(buttons[int.Parse(f)+1]);
            }
        }
        return buttonList.ToArray();
    }

    KMSelectable[] TwitchHandleForcedSolve () {
        List<KMSelectable> buttonList = new List<KMSelectable>();
        foreach(string i in solution) {
            for(int f=2; f<11; f++) {
                if(buttons[f].transform.Find("New Text").GetComponent<TextMesh>().text == i) {
                    buttonList.Add(buttons[f]);
                }
            }
        }
        buttonList.Add(buttons[0]);
        return buttonList.ToArray();
    }
}
