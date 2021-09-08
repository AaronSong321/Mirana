local a = function(t) return t:GetName() end
local c = function(a, b) return a == b end
local a = function(__mira_lpar_1, __mira_lpar_2)
    return __mira_lpar_1 == __mira_lpar_2
end
local b = function(__mira_lpar_it)
    return __mira_lpar_it.count
end
local a,b = function(__mira_olpar_1, __mira_olpar_2) return __mira_olpar_1 > __mira_olpar_2 end, function(__mira_olpar_1) return not __mira_olpar_1 end
do
    local target = a:GetTarget()
    local health = f:GetHealth()
    if target then
        ShootAt(target)
    elseif health then
        print(health)
    end
end
a = a - 1
local __mira_locvar_1 = a.b()[1]:c()
__mira_locvar_1[2] = __mira_locvar_1[2] + 1
local mf = function(d)
    return 3 - 3 + 10 - (3 - 3 * 3 - 3)
end
mf(3)
