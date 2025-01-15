#!/bin/bash

echo "---------------------------------------"
echo "Register ElementMaster"
read
curl -X POST http://localhost:10001/register \
    --header "Content-Type: application/json" \
    -d "{\"Username\":\"elementMaster\", \"Password\":\"elementMaster\"}"



echo "---------------------------------------"
echo "Login ElementMaster"
read
curl -X POST http://localhost:10001/login \
    --header "Content-Type: application/json" \
    -d "{\"Username\":\"elementMaster\", \"Password\":\"elementMaster\"}" | awk \
    -F'"' '/"token"/ {print $4}' > token_element.txt



echo "---------------------------------------" 
echo "ElementMaster acquires package"
read
curl -X POST http://localhost:10001/acquire \
    --header "Authorization: Bearer $(cat token_element.txt)" \
    --header "Content-Type: application/json"



echo "---------------------------------------"
echo "Element Master's Cards (Testing spell effectiveness)"
read
curl -X POST http://localhost:10001/cards \
    --header "Authorization: Bearer $(cat token_element.txt)" \
    --header "Content-Type: application/json" \
    -d "{\"Name\":\"WaterSpell\", \"Type\":\"Water\", \"Power\":20, \"IsSpell\":true, \"Damage\":50}"
curl -X POST http://localhost:10001/cards \
    --header "Authorization: Bearer $(cat token_element.txt)" \
    --header "Content-Type: application/json" \
    -d "{\"Name\":\"FireSpell\", \"Type\":\"Fire\", \"Power\":25, \"IsSpell\":true, \"Damage\":55}"
curl -X POST http://localhost:10001/cards \
    --header "Authorization: Bearer $(cat token_element.txt)" \
    --header "Content-Type: application/json" \
    -d "{\"Name\":\"NormalSpell\", \"Type\":\"Normal\", \"Power\":30, \"IsSpell\":true, \"Damage\":45}"
curl -X POST http://localhost:10001/cards \
    --header "Authorization: Bearer $(cat token_element.txt)" \
    --header "Content-Type: application/json" \
    -d "{\"Name\":\"Knight\", \"Type\":\"Normal\", \"Power\":35, \"IsSpell\":false, \"Damage\":40}"


echo "---------------------------------------"
echo "Check stacks to select cards for decks"
read
curl -X GET http://localhost:10001/stack \
    --header "Authorization: Bearer $(cat token_element.txt)"



echo "---------------------------------------"
echo "Configure decks"
read
curl -X POST http://localhost:10001/deck \
    --header "Authorization: Bearer $(cat token_element.txt)" \
    --header "Content-Type: application/json" \
    -d '[
    {"Name":"WaterSpell","Type":"Water","Power":20,"IsSpell":true,"Damage":50},
    {"Name":"FireSpell","Type":"Fire","Power":25,"IsSpell":true,"Damage":55},
    {"Name":"NormalSpell","Type":"Normal","Power":30,"IsSpell":true,"Damage":45},
    {"Name":"Knight","Type":"Normal","Power":35,"IsSpell":false,"Damage":40}
]'

echo "---------------------------------------"
echo "Scoreboard before Fight"
read
curl -X GET http://localhost:10001/scoreboard



echo "---------------------------------------"
echo "Test: Element vs Pure Monster"
read
curl -X POST http://localhost:10001/battle \
    --header "Authorization: Bearer $(cat token_element.txt)" \
    --header "Content-Type: application/json" \
    -d "\"puremonster\""


echo "---------------------------------------"
echo "Scoreboard"
read
curl -X GET http://localhost:10001/scoreboard



echo "---------------------------------------"
echo "Profile"
read
curl -X GET http://localhost:10001/profile \
    --header "Authorization: Bearer $(cat token_element.txt)"
