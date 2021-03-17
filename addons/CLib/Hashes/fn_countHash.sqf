#include "macros.hpp"
/*
    Community Lib - CLib

    Author: joko // Jonas

    Description:
    Counts amount of Hashes in Hash set

    Parameter(s):
    0: HashSet <Array> (Default: [[], []])

    Returns:
    Amount of Hashes in Hashset <Number>
*/

params [
    ["_hashSet", HASH_NULL, [[]]]
];

count (_hashSet select HASH_KEYS);
