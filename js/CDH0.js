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
                {
                    "type": "text",
                    "name": "Age",
                    "title": "Возраст:",
                    "inputType": "number"
                }, {
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
                {
                    "type": "text",
                    "name": "LengthOfMenopause",
                    "title": "Продолжительность менопаузы",
                    "visibleIf": "{Gender}= 1",
                    "inputType": "number"
                },

            ],
            "colCount": 2
        },

        {
            "type": "text",
            "name": "FamilyStatus",
            "title": "Семейное положение:",
            "inputType": "text"
        },
        {
            "type": "boolean",
            "name": "AggravatedHeredity",
            "title": "Отягощенная наследственность:",
            "labelTrue": "Да",
            "labelFalse": "Нет",
        },
        {
            "type": "boolean",
            "name": "LiveWithFamily",
            "title": "Живёт",
            "labelTrue": "Один",
            "labelFalse": "C семьей",
        },
        {
            "type": "text",
            "name": "Children",
            "title": "Дети:",
            "inputType": "number"
        },
        {
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
        {
            "type": "boolean",
            "name": "HasOccupationalHazards",
            "title": "Проф. Вредности",
            "labelTrue": "Есть",
            "labelFalse": "Нет",
        },
        {
            "type": "text",
            "visibleIf": "{HasOccupationalHazards}= 1",
            "name": "OccupationalHazards",
            "title": "Если есть, то какие:",
            "inputType": "text"
        },
        {
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
        {
            "type": "text",
            "visibleIf": "{Smoking}= 2",
            "name": "NumberOfCigaretts",
            "title": "Число сиграет:",
            "inputType": "number"
        },
        {
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
        {
            "type": "boolean",
            "name": "Dislipidemia",
            "title": "Дислипидемия",
            "labelTrue": "Есть",
            "labelFalse": "Нет",
        },
    ]
};

window.survey = new Survey.Model(json);

survey
    .onComplete

    .add(function (sender, options) {
        survey.mode = "show";
        survey.clear(false);
        document
            .querySelector('#CDHOres')
            .textContent = "Result JSON:\n" + JSON.stringify(sender.data, null, 3);
    });


survey.data = {

};

$("#CDHOsurv").Survey({model: survey, onValueChanged: surveyValueChanged});
