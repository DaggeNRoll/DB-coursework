Survey
    .StylesManager
    .applyTheme("default");

var surveyValueChanged = function (sender, options) {
    var el = document.getElementById(options.name);
    if (el) {
        el.value = options.value;
    }
};

var json = {
    "title": "Критерии исключения",
    "description": "(Для включения пациента в исследование все ответы должны быть \"Нет\")",
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

        {"isRequired": true,
            "type": "boolean",
            "name": "SymptomaticAG",
            "title": "Симптоматическая АГ:",
            "labelTrue": "Да",
            "labelFalse": "Нет",
        },
        {"isRequired": true,
            "type": "boolean",
            "name": "Cardiomyopathy",
            "title": "Кардиомиопатии:",
            "labelTrue": "Да",
            "labelFalse": "Нет",
        },
        {"isRequired": true,
            "type": "boolean",
            "name": "HeartValvePathology",
            "title": "Клапанная патология сердца:",
            "labelTrue": "Да",
            "labelFalse": "Нет",
        },
{           "isRequired": true,
            "type": "boolean",
            "name": "HeartRateAndConductancePathology",
            "title": "Клинически значимые нарушения ритма и проводимости сердца:",
            "labelTrue": "Да",
            "labelFalse": "Нет",
        },
{           "isRequired": true,
            "type": "boolean",
            "name": "EndocrineDisease",
            "title": "Эндокринные заболевания:",
            "labelTrue": "Да",
            "labelFalse": "Нет",
        },
{           "isRequired": true,
            "type": "boolean",
            "name": "ChronicLiverRenalFailure",
            "title": "Хроническая почечная и печеночная недостаточность:",
            "labelTrue": "Да",
            "labelFalse": "Нет",
        },
{           "isRequired": true,
            "type": "boolean",
            "name": "OncoHemoDisease",
            "title": "Онкологические и гематологические заболевания:",
            "labelTrue": "Да",
            "labelFalse": "Нет",
        },
{           "isRequired": true,
            "type": "boolean",
            "name": "CollagenOutbreak",
            "title": "Коллагенозы:",
            "labelTrue": "Да",
            "labelFalse": "Нет",
        },
{           "isRequired": true,
            "type": "boolean",
            "name": "MorbideObesity",
            "title": "Морбидное ожирение (ИМТ>40 кг/м2):",
            "labelTrue": "Да",
            "labelFalse": "Нет",
        },
{           "isRequired": true,
            "type": "boolean",
            "name": "InflammatoryBowelDisease",
            "title": "Наличие в анамнезе воспалительных заболеваний кишечника:",
            "labelTrue": "Да",
            "labelFalse": "Нет",
        },
{           "isRequired": true,
            "type": "boolean",
            "name": "OPP",
            "title": "Острые респираторные заболевания в течение 1 предшествующего месяца:",
            "labelTrue": "Да",
            "labelFalse": "Нет",
        },
{           "isRequired": true,
            "type": "boolean",
            "name": "OperationAntibioticAntiInflamatory",
            "title": "Оперативное лечение, антибактериальная и противовоспалительная терапия в течение 3 предшествующих месяцев:",
            "labelTrue": "Да",
            "labelFalse": "Нет",
        },
{           "isRequired": true,
            "type": "boolean",
            "name": "PsychtropicDrug",
            "title": "Прием психотропных препаратов в течение 3 предшествующих месяцев:",
            "labelTrue": "Да",
            "labelFalse": "Нет",
        },

    ]
};

window.survey3 = new Survey.Model(json);

let rest = sessionStorage.getItem('visit');
function loadState(survey) {
    $.ajax({
        url: '/api/criteriaforexcerprion',
        type: 'GET',
        data: rest,
        contentType: 'application/json;charset=utf-8',
        success: function (response) {
            let res = {};
            res = JSON.parse(response);
            if (res.data)
                survey.data = res.data;

        },
        error: function () {
            alert("Возникла ошибка");
        }

    });
}
survey3.onComplete.add(function (sender, options) {
        survey3.mode = "show";
        survey3.clear(false);
        document.querySelector('#KIskl0res').textContent = "Result JSON:\n" + JSON.stringify(sender.data, null, 3);
    var mySurvey = sender;
    var surveyData = sender.data;
    var xhr = new XMLHttpRequest();
    xhr.open("PUT", "/api/criteriaforexcerprion", true);
    xhr.setRequestHeader("Content-Type", "application/json; charset=utf-8");
    xhr.onload = xhr.onerror = function () {
        if (xhr.status === 200)
        {
            options.showDataSavingSuccess("Успешный успех"); // you may pass a text parameter to show your own text
            // Or you may clear all messages:
            // options.showDataSavingClear();
        } else
        {
            //Error
            options.showDataSavingError("Бан по причине "+ xhr.status); // you may pass a text parameter to show your own text
        }
    };
    xhr.send(JSON.stringify(sender.data));
    });


survey.data = {

};

$("#KIskl0surv").Survey({model: survey3, onValueChanged: surveyValueChanged});
