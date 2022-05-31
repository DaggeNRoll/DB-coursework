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
    "title": "Клинико-демографическая характеристика",
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
            "type": "panel",
            "name": "age_sex",
            "elements": [
                {"isRequired": true,
                    "type": "text",
                    "name": "Age",
                    "title": "Возраст:",
                    "inputType": "number"
                }, {"isRequired": true,
                    "type": "radiogroup",
                    "name": "Gender",
                    "title": "Пол",
                    "choices":
                        [
                            {
                                "value": 0,
                                "text": "М"
                            },
                            {
                                "value": 1,
                                "text": "Ж"
                            }
                        ],
                    "colCount": 2
                },
                {"isRequired": true,
                    "type": "text",
                    "name": "LengthOfMenopause",
                    "title": "Продолжительность менопаузы",
                    "visibleIf": "{Gender}= 1",
                    "inputType": "number"
                },

            ],
            "colCount": 2
        },

        {"isRequired": true,
            "type": "text",
            "name": "FamilyStatus",
            "title": "Семейное положение:",
            "inputType": "text"
        },
        {"isRequired": true,
            "type": "radiogroup",
            "name": "AggravatedHeredity",
            "title": "Отягощенная наследственность:",
            "choices":
                [
                    {
                        "value": 0,
                        "text": "Нет"
                    },
                    {
                        "value": 1,
                        "text": "Да"
                    }
                ],
            "colCount": 2
        },
        {"isRequired": true,
            "type": "boolean",
            "name": "LiveWithFamily",
            "title": "Живёт",
            "labelTrue": "Один",
            "labelFalse": "C семьей",
        },
        {"isRequired": true,
            "type": "text",
            "name": "Children",
            "title": "Дети:",
            "inputType": "number"
        },
        {"isRequired": true,
            "type": "radiogroup",
            "name": "PhysicalActivity",
            "title": "Физическая активность:",
            "choices":
                [
                    {
                        "value": 0,
                        "text": "Регулярная"
                    },
                    {
                        "value": 1,
                        "text": "Не регулярная"
                    },
                    {
                        "value": 2,
                        "text": "Малоподвижный образ жизни"
                    }
                ],

        },
             {
                        "type": "radiogroup",
                        "name": "WorkStatus",
                        "title": "Работает:",
                        "choices":
                            [
                                {
                                    "value": 0,
                                    "text": "Не работает"
                                },
                                {
                                    "value": 1,
                                    "text": "Инвалидность"
                                },
                                {
                                    "value": 2,
                                    "text": "Пенсия"
                                },
                                {
                                    "value": 3,
                                    "text": "Физический труд"
                                },
                                {
                                    "value": 4,
                                    "text": "Офисная работа"
                                },
                                {
                                    "value": 5,
                                    "text": "Фрилансер/ИП"
                                }

                            ],
                        "colCount": 1
                    },
        {"isRequired": true,
            "type": "boolean",
            "name": "HasOccupationalHazards",
            "title": "Проф. Вредности",
            "labelTrue": "Есть",
            "labelFalse": "Нет",
        },
        {"isRequired": true,
            "type": "text",
            "visibleIf": "{HasOccupationalHazards}= 1",
            "name": "OccupationalHazards",
            "title": "Если есть, то какие:",
            "inputType": "text"
        },
        {"isRequired": true,
            "type": "radiogroup",
            "name": "Smoking",
            "title": "Курение:",
            "choices":
            [
                {
                    "value": 0,
                    "text": "Нет"
                },
                {
                    "value": 1,
                    "text": "Бросил более года назад"
                },
                {
                    "value": 2,
                    "text": "Курит"
                }
            ],
        },
        {"isRequired": true,
            "type": "text",
            "visibleIf": "{Smoking}= 2",
            "name": "NumberOfCigaretts",
            "title": "Число сиграет:",
            "inputType": "number"
        },
        {"isRequired": true,
            "type": "radiogroup",
            "name": "Hypertension",
            "title": "Артериальная гипертензия:",
            "choices":
                [
                    {
                        "value": 0,
                        "text": "Нет"
                    },
                    {
                        "value": 1,
                        "text": "1 степень"
                    },
                    {
                        "value": 2,
                        "text": "2 степень"
                    },
                    {
                        "value": 2,
                        "text": "3 степень"
                    }
                ],
        },
        {"isRequired": true,
            "type": "boolean",
            "name": "Dislipidemia",
            "title": "Дислипидемия",
            "labelTrue": "Есть",
            "labelFalse": "Нет",
        },
    ]
};
let visitId = sessionStorage.getItem("visitId");

alert(visitId);



window.survey = new Survey.Model(json);
function loadState(survey) {
    $.ajax({
        url: '/api/kdh/{visitId}',
        type: 'GET',
        //data: rest,
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

survey.onComplete.add(function (sender, options) {
        survey.mode = "show";
        survey.clear(false);
        document.querySelector('#CDHOres').textContent = "Result JSON:\n" + JSON.stringify(sender.data, null, 3);
    var mySurvey = sender;
    var surveyData = sender.data;
    var xhr = new XMLHttpRequest();
    xhr.open("PUT", "/api/kdh/{visitId}", true);
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

$("#CDHOsurv").Survey({model: survey, onValueChanged: surveyValueChanged});
