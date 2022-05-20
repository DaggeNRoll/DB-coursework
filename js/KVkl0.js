Survey
    .StylesManager
    .applyTheme("default");

var surveyValueChanged = function (sender, options) {
    var el = document.getElementById(options.name);
    if (el) {
        el.value = options.value;
    }
};

var json1 = {
    "title": "Критерии включения",
    "description": "(Для включения пациента в исследование все ответы должны быть \"Да\")",
    triggers: [
        {
            type: "complete",
            expression: "{exit1} = 'Yes'"
        }, {
            type: "complete",
            expression: "{exit2} = 'Yes'"
        }
    ],

    "elements": [

        {
            "type": "boolean",
            "name": "AgeBetween40_65",
            "title": "Возраст 40 – 65 лет:",
            "labelTrue": "Да",
            "labelFalse": "Нет",
        },
        {
            "type": "boolean",
            "name": "LowAndModerateRiskOfCardiovascularComplications",
            "title": "Низкий и умеренный риск сердечно – сосудистых осложнений (0-\n" +
                "4% по шкале SCORE):",
            "labelTrue": "Да",
            "labelFalse": "Нет",
        },
        {
            "type": "boolean",
            "name": "ParticipationAgreement",
            "title": "Согласие на участие в исследовании:",
            "labelTrue": "Да",
            "labelFalse": "Нет",
        },

    ]
};

window.survey2 = new Survey.Model(json1);

survey2
    .onComplete

    .add(function (sender, options) {
        survey2.mode = "show";
        survey2.clear(false);
        document
            .querySelector('#KVkl0res')
            .textContent = "Result JSON:\n" + JSON.stringify(sender.data, null, 3);
    });


survey2.data = {

};

$("#KVkl0surv").Survey({model: survey2, onValueChanged: surveyValueChanged});
