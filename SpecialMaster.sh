#!/bin/bash

echo "---------------------------------------"
echo "Register SpecialMaster"
read
curl -X POST http://localhost:10001/register \
    --header "Content-Type: application/json" \
    -d "{\"Username\":\"specialMaster\", \"Password\":\"specialMaster\"}"


echo "---------------------------------------"
echo "Login SpecialMaster"
read
curl -X POST http://localhost:10001/login \
    --header "Content-Type: application/json" \
    -d "{\"Username\":\"specialMaster\", \"Password\":\"specialMaster\"}" | awk \
    -F'"' '/"token"/ {print $4}' > token_special.txt


echo "---------------------------------------"
echo "SpecialMaster acquires package"
read
curl -X POST http://localhost:10001/acquire \
    --header "Authorization: Bearer $(cat token_special.txt)" \
    --header "Content-Type: application/json"


echo "---------------------------------------"
echo "Special Master's Cards (Testing all special interactions)"
read
curl -X POST http://localhost:10001/cards \
    --header "Authorization: Bearer $(cat token_special.txt)" \
    --header "Content-Type: application/json" \
    -d "{\"Name\":\"Goblin\", \"Type\":\"Normal\", \"Power\":15, \"IsSpell\":false, \"Damage\":30}"
curl -X POST http://localhost:10001/cards \
    --header "Authorization: Bearer $(cat token_special.txt)" \
    --header "Content-Type: application/json" \
    -d "{\"Name\":\"Dragon\", \"Type\":\"Fire\", \"Power\":55, \"IsSpell\":false, \"Damage\":70}"
curl -X POST http://localhost:10001/cards \
    --header "Authorization: Bearer $(cat token_special.txt)" \
    --header "Content-Type: application/json" \
    -d "{\"Name\":\"WaterSpell\", \"Type\":\"Water\", \"Power\":40, \"IsSpell\":true, \"Damage\":60}"
curl -X POST http://localhost:10001/cards \
    --header "Authorization: Bearer $(cat token_special.txt)" \
    --header "Content-Type: application/json" \
    -d "{\"Name\":\"Kraken\", \"Type\":\"Water\", \"Power\":50, \"IsSpell\":false, \"Damage\":65}"


echo "---------------------------------------"
echo "Check stacks to select cards for decks"
read
curl -X GET http://localhost:10001/stack \
    --header "Authorization: Bearer $(cat token_special.txt)"


echo "---------------------------------------"
echo "Configure decks"
read
curl -X POST http://localhost:10001/deck \
    --header "Authorization: Bearer $(cat token_special.txt)" \
    --header "Content-Type: application/json" \
    -d '[
    {"Name":"Goblin","Type":"Normal","Power":15,"IsSpell":false,"Damage":30},
    {"Name":"Dragon","Type":"Fire","Power":55,"IsSpell":false,"Damage":70},
    {"Name":"WaterSpell","Type":"Water","Power":40,"IsSpell":true,"Damage":60},
    {"Name":"Kraken","Type":"Water","Power":50,"IsSpell":false,"Damage":65}
]'

echo "---------------------------------------"
echo "Scoreboard before Fight"
read
curl -X GET http://localhost:10001/scoreboard


echo "---------------------------------------"
echo "Test: Special Interactions"
read
curl -X POST http://localhost:10001/battle \
    --header "Authorization: Bearer $(cat token_special.txt)" \
    --header "Content-Type: application/json" \
    -d "\"elementMaster\""

echo "---------------------------------------"
echo "Scoreboard"
read
curl -X GET http://localhost:10001/scoreboard


echo "---------------------------------------"
echo "Profile"
read
curl -X GET http://localhost:10001/profile \
    --header "Authorization: Bearer $(cat token_special.txt)"

