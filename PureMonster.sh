#!/bin/bash

echo "---------------------------------------"
echo "Register PureMonster"
read
curl -X POST http://localhost:10001/register \
    --header "Content-Type: application/json" \
    -d "{\"Username\":\"puremonster\", \"Password\":\"puremonster\"}"


echo "---------------------------------------"
echo "Login PureMonster"
read
curl -X POST http://localhost:10001/login \
    --header "Content-Type: application/json" \
    -d "{\"Username\":\"puremonster\", \"Password\":\"puremonster\"}" | awk \
    -F'"' '/"token"/ {print $4}' > token_puremonster.txt


echo "---------------------------------------"
echo "PureMonster will not acquires package"
read


echo "---------------------------------------"
echo "Monster User's Cards (Testing pure monster interactions)"
read
curl -X POST http://localhost:10001/cards \
    --header "Authorization: Bearer $(cat token_puremonster.txt)" \
    --header "Content-Type: application/json" \
    -d "{\"Name\":\"FireMonster\", \"Type\":\"Fire\", \"Power\":40, \"IsSpell\":false, \"Damage\":60}"
curl -X POST http://localhost:10001/cards \
    --header "Authorization: Bearer $(cat token_puremonster.txt)" \
    --header "Content-Type: application/json" \
    -d "{\"Name\":\"WaterMonster\", \"Type\":\"Water\", \"Power\":35, \"IsSpell\":false, \"Damage\":55}"
curl -X POST http://localhost:10001/cards \
    --header "Authorization: Bearer $(cat token_puremonster.txt)" \
    --header "Content-Type: application/json" \
    -d "{\"Name\":\"NormalMonster\", \"Type\":\"Normal\", \"Power\":30, \"IsSpell\":false, \"Damage\":50}"
curl -X POST http://localhost:10001/cards \
    --header "Authorization: Bearer $(cat token_puremonster.txt)" \
    --header "Content-Type: application/json" \
    -d "{\"Name\":\"StrongMonster\", \"Type\":\"Normal\", \"Power\":45, \"IsSpell\":false, \"Damage\":65}"


echo "---------------------------------------"
echo "Check stacks to select cards for decks"
read
curl -X GET http://localhost:10001/stack \
    --header "Authorization: Bearer $(cat token_puremonster.txt)"


echo "---------------------------------------"
echo "Configure decks"
read
curl -X POST http://localhost:10001/deck \
    --header "Authorization: Bearer $(cat token_puremonster.txt)" \
    --header "Content-Type: application/json" \
    -d '[
    {"Name":"FireMonster","Type":"Fire","Power":40,"IsSpell":false,"Damage":60},
    {"Name":"WaterMonster","Type":"Water","Power":35,"IsSpell":false,"Damage":55},
    {"Name":"NormalMonster","Type":"Normal","Power":30,"IsSpell":false,"Damage":50},
    {"Name":"StrongMonster","Type":"Normal","Power":45,"IsSpell":false,"Damage":65}
]'

echo "---------------------------------------"
echo "Scoreboard before Fight"
read
curl -X GET http://localhost:10001/scoreboard


echo "---------------------------------------"
echo "Scoreboard"
read
curl -X GET http://localhost:10001/scoreboard


echo "---------------------------------------"
echo "Profile"
read
curl -X GET http://localhost:10001/profile \
    --header "Authorization: Bearer $(cat token_puremonster.txt)"

