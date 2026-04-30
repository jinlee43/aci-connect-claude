-- Employee encrypted fields re-encryption migration
-- Generated: 2026-04-29 11:45:25
-- Old: AES-CBC  →  New: AES-256-GCM

BEGIN;

UPDATE public."Employees" SET
    "SsnEncrypted" = NULL,
    "TinEncrypted" = NULL,
    "AlienNumberEncrypted" = NULL,
    "PassportNumberEncrypted" = NULL,
    "DriversLicNumEncrypted" = NULL,
    "BirthDate" = NULL,
    "UpdatedAt" = NOW()
WHERE "Notes" = 'OBID:Emp_20240726_01013';

UPDATE public."Employees" SET
    "SsnEncrypted" = 'txnnie1GKAfoTjvqL/Fxurg6rXf+FBfeX1Sfi80xwgaivI902g==',
    "TinEncrypted" = NULL,
    "AlienNumberEncrypted" = NULL,
    "PassportNumberEncrypted" = NULL,
    "DriversLicNumEncrypted" = NULL,
    "BirthDate" = '1986-01-09',
    "UpdatedAt" = NOW()
WHERE "Notes" = 'OBID:Emp_20240726_01023';

UPDATE public."Employees" SET
    "SsnEncrypted" = 'qQ9Leh8DIjHo8F2zGTfUGI8586dTp3upecbwOW6OB+cwZpjNDg==',
    "TinEncrypted" = NULL,
    "AlienNumberEncrypted" = NULL,
    "PassportNumberEncrypted" = NULL,
    "DriversLicNumEncrypted" = NULL,
    "BirthDate" = '1991-03-18',
    "UpdatedAt" = NOW()
WHERE "Notes" = 'OBID:Emp_20240726_01049';

UPDATE public."Employees" SET
    "SsnEncrypted" = 'fudFnmL/qFN370s/VT91iGayfxAp2O3p4MvoYMUD2LAfmqIm4g==',
    "TinEncrypted" = NULL,
    "AlienNumberEncrypted" = '04MD9SkyUcWz76mbZsOy2tL3Yb/zhRLM7DUSm0y5skuKBG4zgA==',
    "PassportNumberEncrypted" = NULL,
    "DriversLicNumEncrypted" = NULL,
    "BirthDate" = '1973-12-17',
    "UpdatedAt" = NOW()
WHERE "Notes" = 'OBID:Emp_20240726_01034';

UPDATE public."Employees" SET
    "SsnEncrypted" = NULL,
    "TinEncrypted" = NULL,
    "AlienNumberEncrypted" = NULL,
    "PassportNumberEncrypted" = NULL,
    "DriversLicNumEncrypted" = NULL,
    "BirthDate" = NULL,
    "UpdatedAt" = NOW()
WHERE "Notes" = 'OBID:Emp_20240726_01038';

UPDATE public."Employees" SET
    "SsnEncrypted" = '5ocHRxeZPRwz2GFA1hepHNF3K6HTYqKXY1bTMnlYMlTX3u/vdg==',
    "TinEncrypted" = NULL,
    "AlienNumberEncrypted" = NULL,
    "PassportNumberEncrypted" = NULL,
    "DriversLicNumEncrypted" = NULL,
    "BirthDate" = '1984-09-28',
    "UpdatedAt" = NOW()
WHERE "Notes" = 'OBID:Emp_20240726_01030';

UPDATE public."Employees" SET
    "SsnEncrypted" = 'fJXG2IoWZcUJzRl5aDHE/J76nTXtv2nU8/EHfKsrdIFXUMJJYQ==',
    "TinEncrypted" = NULL,
    "AlienNumberEncrypted" = NULL,
    "PassportNumberEncrypted" = NULL,
    "DriversLicNumEncrypted" = NULL,
    "BirthDate" = '1972-08-22',
    "UpdatedAt" = NOW()
WHERE "Notes" = 'OBID:Emp_20240726_01037';

UPDATE public."Employees" SET
    "SsnEncrypted" = '5TLZdLORapcnZ6nzFQtAK27Wr4vuSaiEolWifAeVyaUXJKFxJg==',
    "TinEncrypted" = NULL,
    "AlienNumberEncrypted" = NULL,
    "PassportNumberEncrypted" = NULL,
    "DriversLicNumEncrypted" = NULL,
    "BirthDate" = '1983-04-28',
    "UpdatedAt" = NOW()
WHERE "Notes" = 'OBID:Emp_20240726_01051';

UPDATE public."Employees" SET
    "SsnEncrypted" = 'RwkJTAm9iF+pFCnE5M1kUFpYpDol6EzRYqDstkqv/l6/FlUM8w==',
    "TinEncrypted" = NULL,
    "AlienNumberEncrypted" = NULL,
    "PassportNumberEncrypted" = NULL,
    "DriversLicNumEncrypted" = NULL,
    "BirthDate" = '1977-10-17',
    "UpdatedAt" = NOW()
WHERE "Notes" = 'OBID:Emp_20240726_01070';

UPDATE public."Employees" SET
    "SsnEncrypted" = 'nohIs+k29jDkp7IuDeFiDpaHrf9cRzH7LEp1WSCBZbYQaq+Quw==',
    "TinEncrypted" = NULL,
    "AlienNumberEncrypted" = NULL,
    "PassportNumberEncrypted" = NULL,
    "DriversLicNumEncrypted" = NULL,
    "BirthDate" = '1963-09-01',
    "UpdatedAt" = NOW()
WHERE "Notes" = 'OBID:Emp_20240726_01089';

UPDATE public."Employees" SET
    "SsnEncrypted" = 'tgbR5/zGv/4IWz8YlOaSYexO9YwAjhbdqG6PBhMv85a52piBUg==',
    "TinEncrypted" = NULL,
    "AlienNumberEncrypted" = NULL,
    "PassportNumberEncrypted" = NULL,
    "DriversLicNumEncrypted" = NULL,
    "BirthDate" = '1995-05-12',
    "UpdatedAt" = NOW()
WHERE "Notes" = 'OBID:Emp_20240726_01012';

UPDATE public."Employees" SET
    "SsnEncrypted" = 'lm6IELR++DAht0igforM1QoNdpD9SooIwH5qH1lGk/GjJFSCAgWA',
    "TinEncrypted" = 'tcC6AxeFmdeeS9w1c+jKQqNpLytz4dBF5VIWcb8hXQ==',
    "AlienNumberEncrypted" = 'ACrPsVvOHlPYaXUA9iz8Xgfg2Gar+rhuKOAEC4cBkQ==',
    "PassportNumberEncrypted" = NULL,
    "DriversLicNumEncrypted" = NULL,
    "BirthDate" = '1969-03-06',
    "UpdatedAt" = NOW()
WHERE "Notes" = 'OBID:Emp_20240726_01020';

UPDATE public."Employees" SET
    "SsnEncrypted" = '5VgbLsISKrDALd8CMVGN/hurhnXkVMJcHEbW5C+ItyAPUd72Zg==',
    "TinEncrypted" = NULL,
    "AlienNumberEncrypted" = NULL,
    "PassportNumberEncrypted" = NULL,
    "DriversLicNumEncrypted" = NULL,
    "BirthDate" = '1970-05-03',
    "UpdatedAt" = NOW()
WHERE "Notes" = 'OBID:Emp_20240726_01011';

UPDATE public."Employees" SET
    "SsnEncrypted" = 'LSmFOq9FQcPowi71tx0GfxOcdMjVHhvSuV7xlwfEnu+2sCMDQA==',
    "TinEncrypted" = NULL,
    "AlienNumberEncrypted" = NULL,
    "PassportNumberEncrypted" = NULL,
    "DriversLicNumEncrypted" = NULL,
    "BirthDate" = '1975-05-26',
    "UpdatedAt" = NOW()
WHERE "Notes" = 'OBID:Emp_20240726_01048';

UPDATE public."Employees" SET
    "SsnEncrypted" = 'gCP5jfZ0eZ+9LX2yzHu+s8efjRF9vOFn5VZZMTU629tjDSOkhA==',
    "TinEncrypted" = NULL,
    "AlienNumberEncrypted" = NULL,
    "PassportNumberEncrypted" = NULL,
    "DriversLicNumEncrypted" = NULL,
    "BirthDate" = '1990-07-24',
    "UpdatedAt" = NOW()
WHERE "Notes" = 'OBID:Emp_20240726_01095';

UPDATE public."Employees" SET
    "SsnEncrypted" = 'zRRGCk6mjcrNjZCpaKHSXBCM+8ADxXNRy03pXFfvlur1JjsFyA==',
    "TinEncrypted" = NULL,
    "AlienNumberEncrypted" = NULL,
    "PassportNumberEncrypted" = NULL,
    "DriversLicNumEncrypted" = NULL,
    "BirthDate" = '1992-06-16',
    "UpdatedAt" = NOW()
WHERE "Notes" = 'OBID:Emp_20240726_01052';

UPDATE public."Employees" SET
    "SsnEncrypted" = 'M0F/Zhzm5TfEhmhmIKkNalM6jENTZaUftL6wea91uCaDirWHlQ==',
    "TinEncrypted" = NULL,
    "AlienNumberEncrypted" = NULL,
    "PassportNumberEncrypted" = NULL,
    "DriversLicNumEncrypted" = NULL,
    "BirthDate" = '1978-11-28',
    "UpdatedAt" = NOW()
WHERE "Notes" = 'OBID:Emp_20240726_01063';

UPDATE public."Employees" SET
    "SsnEncrypted" = 'OlnMiCNoH9fKdFfK/qzCFEvCx/Pr1WAslLvc4Eo319wRnxzdgQ==',
    "TinEncrypted" = NULL,
    "AlienNumberEncrypted" = NULL,
    "PassportNumberEncrypted" = NULL,
    "DriversLicNumEncrypted" = NULL,
    "BirthDate" = '1968-01-10',
    "UpdatedAt" = NOW()
WHERE "Notes" = 'OBID:Emp_20240726_01046';

UPDATE public."Employees" SET
    "SsnEncrypted" = 'VAGpFzuhpjUQm5V/ETO+bM31Fldt2MZmUz8qLnWuK0X/UX/xjw==',
    "TinEncrypted" = NULL,
    "AlienNumberEncrypted" = NULL,
    "PassportNumberEncrypted" = NULL,
    "DriversLicNumEncrypted" = NULL,
    "BirthDate" = '2002-09-10',
    "UpdatedAt" = NOW()
WHERE "Notes" = 'OBID:Emp_20240726_01071';

UPDATE public."Employees" SET
    "SsnEncrypted" = NULL,
    "TinEncrypted" = NULL,
    "AlienNumberEncrypted" = NULL,
    "PassportNumberEncrypted" = NULL,
    "DriversLicNumEncrypted" = NULL,
    "BirthDate" = NULL,
    "UpdatedAt" = NOW()
WHERE "Notes" = 'OBID:Emp_20240726_01097';

UPDATE public."Employees" SET
    "SsnEncrypted" = 'BHfBidGWoYuR5I5elPNUx+Gvt/F8/KcLs8HWI6TyGBTAc0GHXQ==',
    "TinEncrypted" = NULL,
    "AlienNumberEncrypted" = '5B8BuYGMCOK+ZptVDWYTC/g7Smg1WeeOpm4A4XIoJ6yxC7vtqA==',
    "PassportNumberEncrypted" = NULL,
    "DriversLicNumEncrypted" = NULL,
    "BirthDate" = '2000-10-19',
    "UpdatedAt" = NOW()
WHERE "Notes" = 'OBID:Emp_20240726_01050';

UPDATE public."Employees" SET
    "SsnEncrypted" = '9zHjqJ8C9wKjwtL2UoKRm35opq6hsXsMMmVAsDpVNFUSG8B7fQ==',
    "TinEncrypted" = NULL,
    "AlienNumberEncrypted" = NULL,
    "PassportNumberEncrypted" = NULL,
    "DriversLicNumEncrypted" = NULL,
    "BirthDate" = '1979-02-12',
    "UpdatedAt" = NOW()
WHERE "Notes" = 'OBID:Emp_20240726_01009';

UPDATE public."Employees" SET
    "SsnEncrypted" = 'MdEpKYi+PkGqdATGnrtIABR/acQNwl2TwAyV2P7KHM9YuGWKPQ==',
    "TinEncrypted" = NULL,
    "AlienNumberEncrypted" = NULL,
    "PassportNumberEncrypted" = NULL,
    "DriversLicNumEncrypted" = NULL,
    "BirthDate" = '1991-09-13',
    "UpdatedAt" = NOW()
WHERE "Notes" = 'OBID:Emp_20240726_01079';

UPDATE public."Employees" SET
    "SsnEncrypted" = 'Bsem72FeFIhfuPDNEW1k9MzhJ2kl9FcpXrd9JfulqM+6dHep0Q==',
    "TinEncrypted" = NULL,
    "AlienNumberEncrypted" = NULL,
    "PassportNumberEncrypted" = NULL,
    "DriversLicNumEncrypted" = NULL,
    "BirthDate" = '1980-11-08',
    "UpdatedAt" = NOW()
WHERE "Notes" = 'OBID:Emp_20240726_01080';

UPDATE public."Employees" SET
    "SsnEncrypted" = 'YGHWrEvfJzmXhfTyI5eARc6aG+wvO58Nra5+6rf3+lkQSlr+RA==',
    "TinEncrypted" = NULL,
    "AlienNumberEncrypted" = NULL,
    "PassportNumberEncrypted" = NULL,
    "DriversLicNumEncrypted" = NULL,
    "BirthDate" = '1953-12-03',
    "UpdatedAt" = NOW()
WHERE "Notes" = 'OBID:Emp_20240726_01094';

UPDATE public."Employees" SET
    "SsnEncrypted" = 'YFnUkxYhbMjFICnIIX3zvmYaoPGssHDACpFzQtB/DMqm2QdBXg==',
    "TinEncrypted" = NULL,
    "AlienNumberEncrypted" = NULL,
    "PassportNumberEncrypted" = NULL,
    "DriversLicNumEncrypted" = NULL,
    "BirthDate" = '1961-07-22',
    "UpdatedAt" = NOW()
WHERE "Notes" = 'OBID:Emp_20240726_01005';

UPDATE public."Employees" SET
    "SsnEncrypted" = 'h8YrVK2sV6nlIt5dmyH8Xxng0pEUGdzAVW3VVY5WTC531KL/rw==',
    "TinEncrypted" = NULL,
    "AlienNumberEncrypted" = NULL,
    "PassportNumberEncrypted" = NULL,
    "DriversLicNumEncrypted" = NULL,
    "BirthDate" = '1972-06-16',
    "UpdatedAt" = NOW()
WHERE "Notes" = 'OBID:Emp_20240726_01007';

UPDATE public."Employees" SET
    "SsnEncrypted" = 'h/E294ggkzezXSvQEhOmRaujdVj7dy0YYcZb/XhX/pN/XJHZ9Q==',
    "TinEncrypted" = NULL,
    "AlienNumberEncrypted" = NULL,
    "PassportNumberEncrypted" = NULL,
    "DriversLicNumEncrypted" = NULL,
    "BirthDate" = '1975-05-19',
    "UpdatedAt" = NOW()
WHERE "Notes" = 'OBID:Emp_20240726_01014';

UPDATE public."Employees" SET
    "SsnEncrypted" = 'oIEROoRCbY82ZBZfvXLM59G2bz6WHta5usVvH2aUdvjHCdIOTA==',
    "TinEncrypted" = NULL,
    "AlienNumberEncrypted" = NULL,
    "PassportNumberEncrypted" = NULL,
    "DriversLicNumEncrypted" = NULL,
    "BirthDate" = '1970-01-09',
    "UpdatedAt" = NOW()
WHERE "Notes" = 'OBID:Emp_20240726_01026';

UPDATE public."Employees" SET
    "SsnEncrypted" = 'TIg479iO4txKMVrsQsMNLd2vH7QsbPbGENmB/WcRv5DbEgQ5ZDBh',
    "TinEncrypted" = NULL,
    "AlienNumberEncrypted" = NULL,
    "PassportNumberEncrypted" = 'tLtBLVvDz6jzSLnr4+Lx4RRbPFq4PxFJYuPLsEb98CaP7A==',
    "DriversLicNumEncrypted" = NULL,
    "BirthDate" = '1950-12-05',
    "UpdatedAt" = NOW()
WHERE "Notes" = 'OBID:Emp_20240726_01004';

UPDATE public."Employees" SET
    "SsnEncrypted" = '9VVr9Kp7knXHq8Wbj4J1+3MTpsFpSMrP5EHOpelWC1V6z1z+Og==',
    "TinEncrypted" = NULL,
    "AlienNumberEncrypted" = NULL,
    "PassportNumberEncrypted" = NULL,
    "DriversLicNumEncrypted" = NULL,
    "BirthDate" = '1980-07-24',
    "UpdatedAt" = NOW()
WHERE "Notes" = 'OBID:Emp_20240726_01024';

UPDATE public."Employees" SET
    "SsnEncrypted" = 'hOowRyMtksdiJnW01tb9aJwSvZ7mJLbaoMc3TGAoA677OIyj6g==',
    "TinEncrypted" = NULL,
    "AlienNumberEncrypted" = NULL,
    "PassportNumberEncrypted" = NULL,
    "DriversLicNumEncrypted" = NULL,
    "BirthDate" = '1975-06-03',
    "UpdatedAt" = NOW()
WHERE "Notes" = 'OBID:Emp_20240726_01021';

UPDATE public."Employees" SET
    "SsnEncrypted" = '5i/hgDLpBcmL01+RsdpOct1FrnPepjaljJzqEBLveOXEc85LNg==',
    "TinEncrypted" = NULL,
    "AlienNumberEncrypted" = NULL,
    "PassportNumberEncrypted" = NULL,
    "DriversLicNumEncrypted" = NULL,
    "BirthDate" = '1971-01-10',
    "UpdatedAt" = NOW()
WHERE "Notes" = 'OBID:Emp_20240726_01025';

UPDATE public."Employees" SET
    "SsnEncrypted" = '5SqsmzQJvtWRlRx3OjDLXLr/oaLD2+NcW6HYFJQWGsZSeaggZA==',
    "TinEncrypted" = NULL,
    "AlienNumberEncrypted" = NULL,
    "PassportNumberEncrypted" = NULL,
    "DriversLicNumEncrypted" = NULL,
    "BirthDate" = '1988-02-20',
    "UpdatedAt" = NOW()
WHERE "Notes" = 'OBID:Emp_20240726_01029';

UPDATE public."Employees" SET
    "SsnEncrypted" = 'yaguE61VN0q3p5lvDSv0Er7h1WPmDhd5pQ8RQaxASpbReDi4wg==',
    "TinEncrypted" = NULL,
    "AlienNumberEncrypted" = NULL,
    "PassportNumberEncrypted" = NULL,
    "DriversLicNumEncrypted" = NULL,
    "BirthDate" = '1976-09-13',
    "UpdatedAt" = NOW()
WHERE "Notes" = 'OBID:Emp_20240726_01027';

UPDATE public."Employees" SET
    "SsnEncrypted" = 'taJEgaYmPDCQjVtZhKsTUPDwHkbeFp21qhKVcKCDsR1stNUKqQ==',
    "TinEncrypted" = NULL,
    "AlienNumberEncrypted" = NULL,
    "PassportNumberEncrypted" = NULL,
    "DriversLicNumEncrypted" = NULL,
    "BirthDate" = '1990-09-20',
    "UpdatedAt" = NOW()
WHERE "Notes" = 'OBID:Emp_20240726_01016';

UPDATE public."Employees" SET
    "SsnEncrypted" = 'vaA7N1er7VNBQj2fw0Pn9rXDbsKdKNdUc3coodtw9UsVYOErcQ==',
    "TinEncrypted" = NULL,
    "AlienNumberEncrypted" = NULL,
    "PassportNumberEncrypted" = NULL,
    "DriversLicNumEncrypted" = NULL,
    "BirthDate" = '1967-06-13',
    "UpdatedAt" = NOW()
WHERE "Notes" = 'OBID:Emp_20240726_01043';

UPDATE public."Employees" SET
    "SsnEncrypted" = '9vhZ8bPqI7EyoO/MAvzySjTg/icEnrt1opJsHznFs1usUfjjUQ==',
    "TinEncrypted" = NULL,
    "AlienNumberEncrypted" = 'jhpNRGn5GVX6vWw+i30MEJIhRQDDs7FhqoQLgBgkbNPzBBr4hw==',
    "PassportNumberEncrypted" = NULL,
    "DriversLicNumEncrypted" = NULL,
    "BirthDate" = '1999-12-28',
    "UpdatedAt" = NOW()
WHERE "Notes" = 'OBID:Emp_20240726_01060';

UPDATE public."Employees" SET
    "SsnEncrypted" = 'JSNsBY1+UcNZBYcZs+4rG5el7N+zU/wGDAikcWtz7OiEzmLQsQ==',
    "TinEncrypted" = NULL,
    "AlienNumberEncrypted" = NULL,
    "PassportNumberEncrypted" = NULL,
    "DriversLicNumEncrypted" = NULL,
    "BirthDate" = '1965-06-06',
    "UpdatedAt" = NOW()
WHERE "Notes" = 'OBID:Emp_20240726_01062';

UPDATE public."Employees" SET
    "SsnEncrypted" = '936QckKd+ik+GOfUUoR+/ar2hEMqvjCuzI7/iIrmv7aqO8Ye/Q==',
    "TinEncrypted" = NULL,
    "AlienNumberEncrypted" = NULL,
    "PassportNumberEncrypted" = NULL,
    "DriversLicNumEncrypted" = NULL,
    "BirthDate" = '1973-11-27',
    "UpdatedAt" = NOW()
WHERE "Notes" = 'OBID:Emp_20240726_01065';

UPDATE public."Employees" SET
    "SsnEncrypted" = 'xwsyzlKZoij22VtZosjsrbjHwo7OL73sXZrJ8rtdbO9IkorkoNGD',
    "TinEncrypted" = NULL,
    "AlienNumberEncrypted" = NULL,
    "PassportNumberEncrypted" = NULL,
    "DriversLicNumEncrypted" = NULL,
    "BirthDate" = '1965-06-04',
    "UpdatedAt" = NOW()
WHERE "Notes" = 'OBID:Emp_20240726_01019';

UPDATE public."Employees" SET
    "SsnEncrypted" = 'U4DNekHJUk2VwCf+jQi+T3v8wZL1Im+RRqgo/A2rUF4IaJqrwMW2',
    "TinEncrypted" = NULL,
    "AlienNumberEncrypted" = NULL,
    "PassportNumberEncrypted" = NULL,
    "DriversLicNumEncrypted" = NULL,
    "BirthDate" = '1979-10-18',
    "UpdatedAt" = NOW()
WHERE "Notes" = 'OBID:Emp_20240726_01018';

UPDATE public."Employees" SET
    "SsnEncrypted" = '/CElp10tuFBDw5HXS/6WCHtZj2yCjesH8w8XwIcK/GEKZKWcEROY',
    "TinEncrypted" = NULL,
    "AlienNumberEncrypted" = NULL,
    "PassportNumberEncrypted" = NULL,
    "DriversLicNumEncrypted" = NULL,
    "BirthDate" = '1992-08-20',
    "UpdatedAt" = NOW()
WHERE "Notes" = 'OBID:Emp_20240726_01015';

UPDATE public."Employees" SET
    "SsnEncrypted" = 'J3A8n28MjFmLcnf+nRstGpu8evP+bMLkZLlx1vaOvzxaCvVqtg==',
    "TinEncrypted" = NULL,
    "AlienNumberEncrypted" = NULL,
    "PassportNumberEncrypted" = NULL,
    "DriversLicNumEncrypted" = NULL,
    "BirthDate" = '1985-05-18',
    "UpdatedAt" = NOW()
WHERE "Notes" = 'OBID:Emp_20240726_01006';

UPDATE public."Employees" SET
    "SsnEncrypted" = '6diQm39ah+qL95YqSZ6Hmx7OmLwvGb0at+MwQrNbPNUOAoIYsQ==',
    "TinEncrypted" = NULL,
    "AlienNumberEncrypted" = NULL,
    "PassportNumberEncrypted" = NULL,
    "DriversLicNumEncrypted" = NULL,
    "BirthDate" = '1966-11-30',
    "UpdatedAt" = NOW()
WHERE "Notes" = 'OBID:Emp_20240726_01067';

UPDATE public."Employees" SET
    "SsnEncrypted" = 'Pk9a1vwGgHq9gI0QMHZM9XEItre8AfcSZtXeqcf5flordpNwQg==',
    "TinEncrypted" = NULL,
    "AlienNumberEncrypted" = NULL,
    "PassportNumberEncrypted" = NULL,
    "DriversLicNumEncrypted" = NULL,
    "BirthDate" = '1965-07-13',
    "UpdatedAt" = NOW()
WHERE "Notes" = 'OBID:Emp_20240726_01096';

UPDATE public."Employees" SET
    "SsnEncrypted" = 'thewF4RyFgXCkoE8cq8tuJl3kixzFeYRQ5lqTTMvkSs9rvBRqw==',
    "TinEncrypted" = NULL,
    "AlienNumberEncrypted" = NULL,
    "PassportNumberEncrypted" = NULL,
    "DriversLicNumEncrypted" = NULL,
    "BirthDate" = '1994-09-22',
    "UpdatedAt" = NOW()
WHERE "Notes" = 'OBID:Emp_20240726_01008';

UPDATE public."Employees" SET
    "SsnEncrypted" = NULL,
    "TinEncrypted" = NULL,
    "AlienNumberEncrypted" = NULL,
    "PassportNumberEncrypted" = NULL,
    "DriversLicNumEncrypted" = NULL,
    "BirthDate" = NULL,
    "UpdatedAt" = NOW()
WHERE "Notes" = 'OBID:Emp_20260210104802036842_HQ';

UPDATE public."Employees" SET
    "SsnEncrypted" = 'aWlvpWcJ8wNVFYnXtmU2jvwjxMXskXRCY6XEvykZ1fG8K9JiHw==',
    "TinEncrypted" = NULL,
    "AlienNumberEncrypted" = NULL,
    "PassportNumberEncrypted" = NULL,
    "DriversLicNumEncrypted" = NULL,
    "BirthDate" = '1973-07-20',
    "UpdatedAt" = NOW()
WHERE "Notes" = 'OBID:Emp_20240726_01056';

UPDATE public."Employees" SET
    "SsnEncrypted" = '93lfttNGYatFZfmFbWcpMxnAmWjVm1Pejq3xK/ns6LXEPLQioA==',
    "TinEncrypted" = NULL,
    "AlienNumberEncrypted" = NULL,
    "PassportNumberEncrypted" = NULL,
    "DriversLicNumEncrypted" = NULL,
    "BirthDate" = '1997-01-21',
    "UpdatedAt" = NOW()
WHERE "Notes" = 'OBID:Emp_20240726_01057';

UPDATE public."Employees" SET
    "SsnEncrypted" = 'dAbskErrgUsWQHosL7geXCftjsmofbQKGsH6LUVJIJxaGtrZOA==',
    "TinEncrypted" = NULL,
    "AlienNumberEncrypted" = NULL,
    "PassportNumberEncrypted" = NULL,
    "DriversLicNumEncrypted" = NULL,
    "BirthDate" = '1995-01-23',
    "UpdatedAt" = NOW()
WHERE "Notes" = 'OBID:Emp_20240726_01058';

UPDATE public."Employees" SET
    "SsnEncrypted" = 'WFLgwrc7d0h3AkG6gxaxqPKjLviKY+cvaJ0EwPVwmVTlsovQyA==',
    "TinEncrypted" = NULL,
    "AlienNumberEncrypted" = NULL,
    "PassportNumberEncrypted" = NULL,
    "DriversLicNumEncrypted" = NULL,
    "BirthDate" = '1955-06-02',
    "UpdatedAt" = NOW()
WHERE "Notes" = 'OBID:Emp_20240726_01059';

UPDATE public."Employees" SET
    "SsnEncrypted" = 'KJukTrMFS2cbIOZmZFz++WfS9Nmf1pyAOqgaomeDlwkhtV5B4A==',
    "TinEncrypted" = NULL,
    "AlienNumberEncrypted" = NULL,
    "PassportNumberEncrypted" = NULL,
    "DriversLicNumEncrypted" = NULL,
    "BirthDate" = '1997-02-22',
    "UpdatedAt" = NOW()
WHERE "Notes" = 'OBID:Emp_20240726_01055';

UPDATE public."Employees" SET
    "SsnEncrypted" = 'DNHXj43eC+68ohw2pkKVlv/rr1/xy1dO3N0fUz7bCLXpgd4cpA==',
    "TinEncrypted" = NULL,
    "AlienNumberEncrypted" = NULL,
    "PassportNumberEncrypted" = NULL,
    "DriversLicNumEncrypted" = NULL,
    "BirthDate" = '1993-09-25',
    "UpdatedAt" = NOW()
WHERE "Notes" = 'OBID:Emp_20240726_01075';

UPDATE public."Employees" SET
    "SsnEncrypted" = 'tyrecvxE27J0gqpIry7RvyVMRYL24mxGlmh3JJo6ap0doMpuhg==',
    "TinEncrypted" = NULL,
    "AlienNumberEncrypted" = NULL,
    "PassportNumberEncrypted" = NULL,
    "DriversLicNumEncrypted" = NULL,
    "BirthDate" = '1986-01-27',
    "UpdatedAt" = NOW()
WHERE "Notes" = 'OBID:Emp_20240726_01069';

UPDATE public."Employees" SET
    "SsnEncrypted" = '7ojU3EiDq1QxYpQdtXgbUL3VU9tMC20Rc2TPaJd/6EUPhrhFbA==',
    "TinEncrypted" = NULL,
    "AlienNumberEncrypted" = NULL,
    "PassportNumberEncrypted" = NULL,
    "DriversLicNumEncrypted" = NULL,
    "BirthDate" = '1970-02-16',
    "UpdatedAt" = NOW()
WHERE "Notes" = 'OBID:Emp_20240726_01077';

UPDATE public."Employees" SET
    "SsnEncrypted" = NULL,
    "TinEncrypted" = NULL,
    "AlienNumberEncrypted" = NULL,
    "PassportNumberEncrypted" = NULL,
    "DriversLicNumEncrypted" = NULL,
    "BirthDate" = NULL,
    "UpdatedAt" = NOW()
WHERE "Notes" = 'OBID:Emp_20240726_01003';

UPDATE public."Employees" SET
    "SsnEncrypted" = 'Ca4DUljOJGL55jANO9lfCJstRkaATurqMcEtFLUFGd1KsX143g==',
    "TinEncrypted" = NULL,
    "AlienNumberEncrypted" = NULL,
    "PassportNumberEncrypted" = NULL,
    "DriversLicNumEncrypted" = NULL,
    "BirthDate" = '1991-05-16',
    "UpdatedAt" = NOW()
WHERE "Notes" = 'OBID:Emp_20240726_01081';

UPDATE public."Employees" SET
    "SsnEncrypted" = '+lmYjlXV4ZvMlHHB3JCeYw7ZXqAuKlTiGF7fn1ITMYHwO/xuowtk',
    "TinEncrypted" = NULL,
    "AlienNumberEncrypted" = NULL,
    "PassportNumberEncrypted" = NULL,
    "DriversLicNumEncrypted" = NULL,
    "BirthDate" = '1981-03-16',
    "UpdatedAt" = NOW()
WHERE "Notes" = 'OBID:Emp_20240726_01066';

UPDATE public."Employees" SET
    "SsnEncrypted" = 'C3J8InHnfZAKDC5RmJhGtJtN/NhP9Id/hwRVwy3Xfow7kfGbXwqA',
    "TinEncrypted" = NULL,
    "AlienNumberEncrypted" = NULL,
    "PassportNumberEncrypted" = NULL,
    "DriversLicNumEncrypted" = NULL,
    "BirthDate" = '1972-01-27',
    "UpdatedAt" = NOW()
WHERE "Notes" = 'OBID:Emp_20240726_01078';

UPDATE public."Employees" SET
    "SsnEncrypted" = 'lgx58IIRRV0rKct4Ebh6OcRz19MD2vJfOqPnlIHL8fjAPUOnrw==',
    "TinEncrypted" = NULL,
    "AlienNumberEncrypted" = NULL,
    "PassportNumberEncrypted" = NULL,
    "DriversLicNumEncrypted" = NULL,
    "BirthDate" = '1968-02-15',
    "UpdatedAt" = NOW()
WHERE "Notes" = 'OBID:Emp_20240726_01082';

UPDATE public."Employees" SET
    "SsnEncrypted" = 'nXIBNmTNtMpUnWjFJe7O1xMe9lgft+9+7ttDBORPG2zBZxmqKA==',
    "TinEncrypted" = NULL,
    "AlienNumberEncrypted" = NULL,
    "PassportNumberEncrypted" = NULL,
    "DriversLicNumEncrypted" = NULL,
    "BirthDate" = '1974-05-27',
    "UpdatedAt" = NOW()
WHERE "Notes" = 'OBID:Emp_20240726_01068';

UPDATE public."Employees" SET
    "SsnEncrypted" = 'uY2sORFOYVp8NaM1zsUyE8hudPXs6Uel3+jzRa1cVUa5OjhhvQ==',
    "TinEncrypted" = NULL,
    "AlienNumberEncrypted" = NULL,
    "PassportNumberEncrypted" = NULL,
    "DriversLicNumEncrypted" = NULL,
    "BirthDate" = '1999-12-11',
    "UpdatedAt" = NOW()
WHERE "Notes" = 'OBID:Emp_20240726_01076';

UPDATE public."Employees" SET
    "SsnEncrypted" = 'rGaMmXYxd3YIl5LCLNMSe9+kqFR6MEOoRLyQTZS1iID1M5NhEw==',
    "TinEncrypted" = NULL,
    "AlienNumberEncrypted" = NULL,
    "PassportNumberEncrypted" = NULL,
    "DriversLicNumEncrypted" = NULL,
    "BirthDate" = '1998-07-24',
    "UpdatedAt" = NOW()
WHERE "Notes" = 'OBID:Emp_20240726_01073';

UPDATE public."Employees" SET
    "SsnEncrypted" = '0ND2mGLa/dMRrBI/SeZiFPATb2ZE108OE/QK6cdX1oyC/j3ivQ==',
    "TinEncrypted" = NULL,
    "AlienNumberEncrypted" = NULL,
    "PassportNumberEncrypted" = NULL,
    "DriversLicNumEncrypted" = NULL,
    "BirthDate" = '1964-01-05',
    "UpdatedAt" = NOW()
WHERE "Notes" = 'OBID:Emp_20240726_01074';

UPDATE public."Employees" SET
    "SsnEncrypted" = 'lv5lekKInC0KNjgeXqmliX/3WziZWz1uEQENC+f3EWn68mgwfg==',
    "TinEncrypted" = NULL,
    "AlienNumberEncrypted" = NULL,
    "PassportNumberEncrypted" = NULL,
    "DriversLicNumEncrypted" = NULL,
    "BirthDate" = '1967-10-06',
    "UpdatedAt" = NOW()
WHERE "Notes" = 'OBID:Emp_20240726_01001';

UPDATE public."Employees" SET
    "SsnEncrypted" = 'oyZMIrR+XXIEr24MUosXCUUqSV/p1FyCRBJ6KUrlGAr5ZmZmSA==',
    "TinEncrypted" = NULL,
    "AlienNumberEncrypted" = NULL,
    "PassportNumberEncrypted" = NULL,
    "DriversLicNumEncrypted" = NULL,
    "BirthDate" = '1995-06-16',
    "UpdatedAt" = NOW()
WHERE "Notes" = 'OBID:Emp_20240726_01072';

UPDATE public."Employees" SET
    "SsnEncrypted" = 'y4L6W+lIYUzoMGTZSj+SRkts1bXfOaSANP1iqFdDylK9fnfn9A==',
    "TinEncrypted" = NULL,
    "AlienNumberEncrypted" = NULL,
    "PassportNumberEncrypted" = NULL,
    "DriversLicNumEncrypted" = NULL,
    "BirthDate" = '1978-07-05',
    "UpdatedAt" = NOW()
WHERE "Notes" = 'OBID:Emp_20240726_01088';

UPDATE public."Employees" SET
    "SsnEncrypted" = 'IjCELKX7q3GDZMvb30y4pW238YyKPs4l0jK/PTblcYXE4T/ECQ==',
    "TinEncrypted" = NULL,
    "AlienNumberEncrypted" = NULL,
    "PassportNumberEncrypted" = NULL,
    "DriversLicNumEncrypted" = NULL,
    "BirthDate" = '1993-04-29',
    "UpdatedAt" = NOW()
WHERE "Notes" = 'OBID:Emp_20240726_01085';

UPDATE public."Employees" SET
    "SsnEncrypted" = 'isrIFN/VzV9ipD1L53IBZ+y3viW1hHSz8eTU+j9QwOsOMhNFwQ==',
    "TinEncrypted" = NULL,
    "AlienNumberEncrypted" = NULL,
    "PassportNumberEncrypted" = NULL,
    "DriversLicNumEncrypted" = NULL,
    "BirthDate" = '1967-06-02',
    "UpdatedAt" = NOW()
WHERE "Notes" = 'OBID:Emp_20240726_01086';

UPDATE public."Employees" SET
    "SsnEncrypted" = '5Zq4efyPcZXxsEZlt9wFTWJ1ikuMebPOHLy1w3qeWsthWlq+LQ==',
    "TinEncrypted" = NULL,
    "AlienNumberEncrypted" = NULL,
    "PassportNumberEncrypted" = NULL,
    "DriversLicNumEncrypted" = NULL,
    "BirthDate" = '1972-01-13',
    "UpdatedAt" = NOW()
WHERE "Notes" = 'OBID:Emp_20240726_01090';

UPDATE public."Employees" SET
    "SsnEncrypted" = '5LtAgjnDGHrlfvO0RlP3ACaAO7TBFe0RPRe2l9ZhTug7WRZKrg==',
    "TinEncrypted" = NULL,
    "AlienNumberEncrypted" = NULL,
    "PassportNumberEncrypted" = NULL,
    "DriversLicNumEncrypted" = NULL,
    "BirthDate" = '1979-01-06',
    "UpdatedAt" = NOW()
WHERE "Notes" = 'OBID:Emp_20240726_01091';

UPDATE public."Employees" SET
    "SsnEncrypted" = 'yovrbAovLqZ1klYSFo5IgIPdJVYqTFMweIk5lDlmPtfGh+Hz6DpM',
    "TinEncrypted" = NULL,
    "AlienNumberEncrypted" = NULL,
    "PassportNumberEncrypted" = NULL,
    "DriversLicNumEncrypted" = NULL,
    "BirthDate" = '1957-03-29',
    "UpdatedAt" = NOW()
WHERE "Notes" = 'OBID:Emp_20240726_01092';

UPDATE public."Employees" SET
    "SsnEncrypted" = 'FyHHtZQbEtYli2K65bWJAfUfkTBzcqV0/reZpBNg17G0UFvIHg==',
    "TinEncrypted" = NULL,
    "AlienNumberEncrypted" = NULL,
    "PassportNumberEncrypted" = NULL,
    "DriversLicNumEncrypted" = NULL,
    "BirthDate" = '1970-04-04',
    "UpdatedAt" = NOW()
WHERE "Notes" = 'OBID:Emp_20240726_01031';

UPDATE public."Employees" SET
    "SsnEncrypted" = 'Uk1aJrU0gVBBSauYLY1lDuiGGxT1SaJ7ptnIuJWRzujmzo24kA==',
    "TinEncrypted" = NULL,
    "AlienNumberEncrypted" = NULL,
    "PassportNumberEncrypted" = NULL,
    "DriversLicNumEncrypted" = NULL,
    "BirthDate" = '1980-04-05',
    "UpdatedAt" = NOW()
WHERE "Notes" = 'OBID:Emp_20240726_01035';

UPDATE public."Employees" SET
    "SsnEncrypted" = 'L1Tm6eRrKZ7iZY5icb5ktoT+4dQSzeCRp0Z+oeRt9PejkQjniQ==',
    "TinEncrypted" = NULL,
    "AlienNumberEncrypted" = NULL,
    "PassportNumberEncrypted" = NULL,
    "DriversLicNumEncrypted" = NULL,
    "BirthDate" = '1976-07-04',
    "UpdatedAt" = NOW()
WHERE "Notes" = 'OBID:Emp_20240726_01047';

UPDATE public."Employees" SET
    "SsnEncrypted" = '7owNpxdz7htwnkNJaYSMU8YELtDx0uk6jhnheJCBxNPsiSpEAw==',
    "TinEncrypted" = NULL,
    "AlienNumberEncrypted" = NULL,
    "PassportNumberEncrypted" = NULL,
    "DriversLicNumEncrypted" = NULL,
    "BirthDate" = '1967-06-01',
    "UpdatedAt" = NOW()
WHERE "Notes" = 'OBID:Emp_20240726_01083';

UPDATE public."Employees" SET
    "SsnEncrypted" = 'WfXtwbYpbNuyZT2u3Zq6ciRe5GAesW4pRJSyWDitvUc7RjxT+Q==',
    "TinEncrypted" = NULL,
    "AlienNumberEncrypted" = NULL,
    "PassportNumberEncrypted" = NULL,
    "DriversLicNumEncrypted" = NULL,
    "BirthDate" = '1963-11-23',
    "UpdatedAt" = NOW()
WHERE "Notes" = 'OBID:Emp_20240726_01039';

UPDATE public."Employees" SET
    "SsnEncrypted" = 'o+E5/kKM7hMvlSgp15yEaD5/QAUlr/jWQaz7E72cHgeOw5VvvQ==',
    "TinEncrypted" = NULL,
    "AlienNumberEncrypted" = NULL,
    "PassportNumberEncrypted" = NULL,
    "DriversLicNumEncrypted" = NULL,
    "BirthDate" = '1960-02-11',
    "UpdatedAt" = NOW()
WHERE "Notes" = 'OBID:Emp_20240726_01036';

UPDATE public."Employees" SET
    "SsnEncrypted" = 'B7On8/cAqxiSTpXIuq+SvZkzsD6YQOQwr969FztCxfJa9KDksQ==',
    "TinEncrypted" = NULL,
    "AlienNumberEncrypted" = NULL,
    "PassportNumberEncrypted" = NULL,
    "DriversLicNumEncrypted" = NULL,
    "BirthDate" = '1981-09-05',
    "UpdatedAt" = NOW()
WHERE "Notes" = 'OBID:Emp_20240726_01042';

UPDATE public."Employees" SET
    "SsnEncrypted" = 'V8WhYTX30/0wvegU9P7WupRUyWMo45b2XdAow7KOX06D3+kQkQ==',
    "TinEncrypted" = NULL,
    "AlienNumberEncrypted" = NULL,
    "PassportNumberEncrypted" = NULL,
    "DriversLicNumEncrypted" = NULL,
    "BirthDate" = '1970-07-22',
    "UpdatedAt" = NOW()
WHERE "Notes" = 'OBID:Emp_20240726_01040';

UPDATE public."Employees" SET
    "SsnEncrypted" = 'aj3somVkT/PajKe2ExniYc+tKf1VJpz8QnZjbmnQNo3dv0AjMw==',
    "TinEncrypted" = NULL,
    "AlienNumberEncrypted" = NULL,
    "PassportNumberEncrypted" = NULL,
    "DriversLicNumEncrypted" = NULL,
    "BirthDate" = '1973-01-25',
    "UpdatedAt" = NOW()
WHERE "Notes" = 'OBID:Emp_20240726_01044';

UPDATE public."Employees" SET
    "SsnEncrypted" = 'Og6aO3b2V0GPoKSdoHwZkuYFU6BWGil66lQ5kyPaUIm8nGa7yw==',
    "TinEncrypted" = NULL,
    "AlienNumberEncrypted" = NULL,
    "PassportNumberEncrypted" = NULL,
    "DriversLicNumEncrypted" = NULL,
    "BirthDate" = '1969-06-25',
    "UpdatedAt" = NOW()
WHERE "Notes" = 'OBID:Emp_20240726_01045';

UPDATE public."Employees" SET
    "SsnEncrypted" = 'Nnw2Ug3zncucFHbKIFAe3F3TuXJNF9Jb9VZEngrn6X0OecdrqQ==',
    "TinEncrypted" = NULL,
    "AlienNumberEncrypted" = NULL,
    "PassportNumberEncrypted" = 'oQBial3GvwISu+GXVz03blC/nEmPcIuXLI8Ln3k6A09UenP+',
    "DriversLicNumEncrypted" = NULL,
    "BirthDate" = '1977-02-25',
    "UpdatedAt" = NOW()
WHERE "Notes" = 'OBID:Emp_20240726_01002';

UPDATE public."Employees" SET
    "SsnEncrypted" = '7dn2qiIh0ZorzulD2yad85eoAtMBp5DaOLSWxq3qRG3Dmj4LMJCT',
    "TinEncrypted" = NULL,
    "AlienNumberEncrypted" = NULL,
    "PassportNumberEncrypted" = NULL,
    "DriversLicNumEncrypted" = NULL,
    "BirthDate" = '1967-02-15',
    "UpdatedAt" = NOW()
WHERE "Notes" = 'OBID:Emp_20240726_01032';

UPDATE public."Employees" SET
    "SsnEncrypted" = 'yTGhH8kxrDqOjx44G9s18XLjAJdL5Yc1xtJhHjGVBvK5ktPuRVT7',
    "TinEncrypted" = NULL,
    "AlienNumberEncrypted" = NULL,
    "PassportNumberEncrypted" = NULL,
    "DriversLicNumEncrypted" = NULL,
    "BirthDate" = '1970-01-10',
    "UpdatedAt" = NOW()
WHERE "Notes" = 'OBID:Emp_20240726_01061';

UPDATE public."Employees" SET
    "SsnEncrypted" = 'RXtsncd93l/0MSzG6SUwZpq8VVeVV2FUcvC4J2YeGqLc1gbymmMg',
    "TinEncrypted" = NULL,
    "AlienNumberEncrypted" = NULL,
    "PassportNumberEncrypted" = NULL,
    "DriversLicNumEncrypted" = NULL,
    "BirthDate" = '1965-08-01',
    "UpdatedAt" = NOW()
WHERE "Notes" = 'OBID:Emp_20240726_01041';

UPDATE public."Employees" SET
    "SsnEncrypted" = 'Bsg7YPf0Vk9QS7efyGWmVojLuZLpXYUKpger1xE9p8wuct9FE5NG',
    "TinEncrypted" = NULL,
    "AlienNumberEncrypted" = NULL,
    "PassportNumberEncrypted" = NULL,
    "DriversLicNumEncrypted" = NULL,
    "BirthDate" = '1983-09-15',
    "UpdatedAt" = NOW()
WHERE "Notes" = 'OBID:Emp_20240726_01093';

UPDATE public."Employees" SET
    "SsnEncrypted" = 'ZSbpa5VpZPulXZDWPxF3OlRG+9Y97T3dZyKwzx7WGcGefoI/qQ==',
    "TinEncrypted" = NULL,
    "AlienNumberEncrypted" = NULL,
    "PassportNumberEncrypted" = NULL,
    "DriversLicNumEncrypted" = NULL,
    "BirthDate" = NULL,
    "UpdatedAt" = NOW()
WHERE "Notes" = 'OBID:Emp_20241113015429085532';

UPDATE public."Employees" SET
    "SsnEncrypted" = '+fX/EIxww1jiIoRqcBLW4zXTPMq+fLpXTTvWjRshYEOc9wzcgQ==',
    "TinEncrypted" = NULL,
    "AlienNumberEncrypted" = NULL,
    "PassportNumberEncrypted" = NULL,
    "DriversLicNumEncrypted" = NULL,
    "BirthDate" = '1968-10-19',
    "UpdatedAt" = NOW()
WHERE "Notes" = 'OBID:Emp_20240726_01033';

UPDATE public."Employees" SET
    "SsnEncrypted" = 'pSch4KtLMEpGcgyTRs7Wb9c886R+Azj5MV4uHBfQA3fyrqouVdKy',
    "TinEncrypted" = NULL,
    "AlienNumberEncrypted" = NULL,
    "PassportNumberEncrypted" = NULL,
    "DriversLicNumEncrypted" = NULL,
    "BirthDate" = '1991-05-06',
    "UpdatedAt" = NOW()
WHERE "Notes" = 'OBID:Emp_20241113011533968465';

UPDATE public."Employees" SET
    "SsnEncrypted" = NULL,
    "TinEncrypted" = NULL,
    "AlienNumberEncrypted" = NULL,
    "PassportNumberEncrypted" = NULL,
    "DriversLicNumEncrypted" = NULL,
    "BirthDate" = NULL,
    "UpdatedAt" = NOW()
WHERE "Notes" = 'OBID:Emp_20250924021111469913_HQ';

UPDATE public."Employees" SET
    "SsnEncrypted" = NULL,
    "TinEncrypted" = NULL,
    "AlienNumberEncrypted" = NULL,
    "PassportNumberEncrypted" = NULL,
    "DriversLicNumEncrypted" = NULL,
    "BirthDate" = NULL,
    "UpdatedAt" = NOW()
WHERE "Notes" = 'OBID:Emp_20250924020707213852_HQ';

UPDATE public."Employees" SET
    "SsnEncrypted" = NULL,
    "TinEncrypted" = NULL,
    "AlienNumberEncrypted" = NULL,
    "PassportNumberEncrypted" = NULL,
    "DriversLicNumEncrypted" = NULL,
    "BirthDate" = NULL,
    "UpdatedAt" = NOW()
WHERE "Notes" = 'OBID:Emp_20250924020921823752_HQ';

UPDATE public."Employees" SET
    "SsnEncrypted" = 'Z1kcXAQYKaoub3aNISI3DsC0l5pn88HAmgoICOLJzVdrV6Yzz3Yg',
    "TinEncrypted" = NULL,
    "AlienNumberEncrypted" = NULL,
    "PassportNumberEncrypted" = NULL,
    "DriversLicNumEncrypted" = NULL,
    "BirthDate" = '1992-02-06',
    "UpdatedAt" = NOW()
WHERE "Notes" = 'OBID:Emp_20241114041559120307';

UPDATE public."Employees" SET
    "SsnEncrypted" = NULL,
    "TinEncrypted" = NULL,
    "AlienNumberEncrypted" = NULL,
    "PassportNumberEncrypted" = NULL,
    "DriversLicNumEncrypted" = NULL,
    "BirthDate" = NULL,
    "UpdatedAt" = NOW()
WHERE "Notes" = 'OBID:Emp_20250924020614407026_HQ';

UPDATE public."Employees" SET
    "SsnEncrypted" = NULL,
    "TinEncrypted" = NULL,
    "AlienNumberEncrypted" = NULL,
    "PassportNumberEncrypted" = NULL,
    "DriversLicNumEncrypted" = NULL,
    "BirthDate" = NULL,
    "UpdatedAt" = NOW()
WHERE "Notes" = 'OBID:Emp_20250924020845292930_HQ';

UPDATE public."Employees" SET
    "SsnEncrypted" = 'l7OBXbHUahNKzCqK/3JZAT6g7B/jFc0xvARxAZApm88w8hu2ZLwN',
    "TinEncrypted" = NULL,
    "AlienNumberEncrypted" = NULL,
    "PassportNumberEncrypted" = NULL,
    "DriversLicNumEncrypted" = NULL,
    "BirthDate" = '1966-05-10',
    "UpdatedAt" = NOW()
WHERE "Notes" = 'OBID:Emp_20241113094833420330';

UPDATE public."Employees" SET
    "SsnEncrypted" = NULL,
    "TinEncrypted" = NULL,
    "AlienNumberEncrypted" = NULL,
    "PassportNumberEncrypted" = NULL,
    "DriversLicNumEncrypted" = NULL,
    "BirthDate" = NULL,
    "UpdatedAt" = NOW()
WHERE "Notes" = 'OBID:Emp_20250924021235669546_HQ';

UPDATE public."Employees" SET
    "SsnEncrypted" = NULL,
    "TinEncrypted" = NULL,
    "AlienNumberEncrypted" = NULL,
    "PassportNumberEncrypted" = NULL,
    "DriversLicNumEncrypted" = NULL,
    "BirthDate" = NULL,
    "UpdatedAt" = NOW()
WHERE "Notes" = 'OBID:Emp_20250924021145949535_HQ';

UPDATE public."Employees" SET
    "SsnEncrypted" = NULL,
    "TinEncrypted" = NULL,
    "AlienNumberEncrypted" = NULL,
    "PassportNumberEncrypted" = NULL,
    "DriversLicNumEncrypted" = NULL,
    "BirthDate" = NULL,
    "UpdatedAt" = NOW()
WHERE "Notes" = 'OBID:Emp_20250924020753404576_HQ';

UPDATE public."Employees" SET
    "SsnEncrypted" = NULL,
    "TinEncrypted" = NULL,
    "AlienNumberEncrypted" = NULL,
    "PassportNumberEncrypted" = NULL,
    "DriversLicNumEncrypted" = NULL,
    "BirthDate" = NULL,
    "UpdatedAt" = NOW()
WHERE "Notes" = 'OBID:Emp_20250924020959197742_HQ';

UPDATE public."Employees" SET
    "SsnEncrypted" = 'JtOfGstNWKIIZqZWZpcZrpXWrFs/yUC0wREfiQO1Ym/pzJVPvx5Y',
    "TinEncrypted" = NULL,
    "AlienNumberEncrypted" = NULL,
    "PassportNumberEncrypted" = NULL,
    "DriversLicNumEncrypted" = NULL,
    "BirthDate" = '2024-11-04',
    "UpdatedAt" = NOW()
WHERE "Notes" = 'OBID:Emp_20241113012332438737';

UPDATE public."Employees" SET
    "SsnEncrypted" = 'NQ++cplxiyHiBQi9F+B7rIlWNwFgkRJBSTrlRgHlIH87YLDQkZFv',
    "TinEncrypted" = NULL,
    "AlienNumberEncrypted" = NULL,
    "PassportNumberEncrypted" = NULL,
    "DriversLicNumEncrypted" = NULL,
    "BirthDate" = '1985-04-05',
    "UpdatedAt" = NOW()
WHERE "Notes" = 'OBID:Emp_20240726_01022';

UPDATE public."Employees" SET
    "SsnEncrypted" = NULL,
    "TinEncrypted" = NULL,
    "AlienNumberEncrypted" = NULL,
    "PassportNumberEncrypted" = NULL,
    "DriversLicNumEncrypted" = NULL,
    "BirthDate" = NULL,
    "UpdatedAt" = NOW()
WHERE "Notes" = 'OBID:Emp_20260210105757892685_HQ';

UPDATE public."Employees" SET
    "SsnEncrypted" = NULL,
    "TinEncrypted" = NULL,
    "AlienNumberEncrypted" = NULL,
    "PassportNumberEncrypted" = NULL,
    "DriversLicNumEncrypted" = NULL,
    "BirthDate" = NULL,
    "UpdatedAt" = NOW()
WHERE "Notes" = 'OBID:Emp_20250924021414037532_HQ';

UPDATE public."Employees" SET
    "SsnEncrypted" = NULL,
    "TinEncrypted" = NULL,
    "AlienNumberEncrypted" = NULL,
    "PassportNumberEncrypted" = NULL,
    "DriversLicNumEncrypted" = NULL,
    "BirthDate" = NULL,
    "UpdatedAt" = NOW()
WHERE "Notes" = 'OBID:Emp_20250924021653821497_HQ';

UPDATE public."Employees" SET
    "SsnEncrypted" = 'ppgj1Sb5524DvB3rLdAj32AZWFWUBrKEgPRPDK+oFoflnGE6pYkX',
    "TinEncrypted" = NULL,
    "AlienNumberEncrypted" = NULL,
    "PassportNumberEncrypted" = NULL,
    "DriversLicNumEncrypted" = NULL,
    "BirthDate" = '1991-12-31',
    "UpdatedAt" = NOW()
WHERE "Notes" = 'OBID:Emp_20240726_01084';

UPDATE public."Employees" SET
    "SsnEncrypted" = 'KlK7B1e188TcO/8ccV43M5V5EpsQ2hMo7faOO+XaKMqWaGO9+ECS',
    "TinEncrypted" = 'u0BZpxf5UiqnfwUEHOmM+RBwkqpG88LkkydYpI+YaFM=',
    "AlienNumberEncrypted" = NULL,
    "PassportNumberEncrypted" = NULL,
    "DriversLicNumEncrypted" = NULL,
    "BirthDate" = '1995-09-08',
    "UpdatedAt" = NOW()
WHERE "Notes" = 'OBID:Emp_20240726_01010';

UPDATE public."Employees" SET
    "SsnEncrypted" = '8ZenhyCMQ/HomcdUS05eD6UTBImm7uBPG7I5odi+2QS+Pv+eixZA',
    "TinEncrypted" = NULL,
    "AlienNumberEncrypted" = NULL,
    "PassportNumberEncrypted" = NULL,
    "DriversLicNumEncrypted" = NULL,
    "BirthDate" = '1989-04-23',
    "UpdatedAt" = NOW()
WHERE "Notes" = 'OBID:Emp_20240726_01054';

UPDATE public."Employees" SET
    "SsnEncrypted" = 'FUYsMIvVyhFMwvtvOhhhYWm408z7DC9M4n0/BNZSPXwTN7Xf9j8p',
    "TinEncrypted" = NULL,
    "AlienNumberEncrypted" = NULL,
    "PassportNumberEncrypted" = NULL,
    "DriversLicNumEncrypted" = NULL,
    "BirthDate" = '1996-12-09',
    "UpdatedAt" = NOW()
WHERE "Notes" = 'OBID:Emp_20240726_01087';

UPDATE public."Employees" SET
    "SsnEncrypted" = 'Ep8nCtmfdl45tjJfKQ4IRmKf9+SeCRX+A/K6QmKI+s9G+hRCdO2c',
    "TinEncrypted" = NULL,
    "AlienNumberEncrypted" = NULL,
    "PassportNumberEncrypted" = NULL,
    "DriversLicNumEncrypted" = NULL,
    "BirthDate" = '1990-05-07',
    "UpdatedAt" = NOW()
WHERE "Notes" = 'OBID:Emp_20240726_01064';

UPDATE public."Employees" SET
    "SsnEncrypted" = 'GCIUT/Nxr846banGcS6N8UHPRHB+7gunGCYbi2ZYjUEeUhwXCK1d',
    "TinEncrypted" = NULL,
    "AlienNumberEncrypted" = NULL,
    "PassportNumberEncrypted" = NULL,
    "DriversLicNumEncrypted" = NULL,
    "BirthDate" = '1985-03-22',
    "UpdatedAt" = NOW()
WHERE "Notes" = 'OBID:Emp_20240726_01028';

UPDATE public."Employees" SET
    "SsnEncrypted" = NULL,
    "TinEncrypted" = NULL,
    "AlienNumberEncrypted" = NULL,
    "PassportNumberEncrypted" = NULL,
    "DriversLicNumEncrypted" = NULL,
    "BirthDate" = NULL,
    "UpdatedAt" = NOW()
WHERE "Notes" = 'OBID:Emp_20250924021325061244_HQ';

UPDATE public."Employees" SET
    "SsnEncrypted" = NULL,
    "TinEncrypted" = NULL,
    "AlienNumberEncrypted" = NULL,
    "PassportNumberEncrypted" = NULL,
    "DriversLicNumEncrypted" = NULL,
    "BirthDate" = NULL,
    "UpdatedAt" = NOW()
WHERE "Notes" = 'OBID:Emp_20250924021043062244_HQ';

UPDATE public."Employees" SET
    "SsnEncrypted" = NULL,
    "TinEncrypted" = NULL,
    "AlienNumberEncrypted" = NULL,
    "PassportNumberEncrypted" = NULL,
    "DriversLicNumEncrypted" = NULL,
    "BirthDate" = NULL,
    "UpdatedAt" = NOW()
WHERE "Notes" = 'OBID:Emp_20250924021824732740_HQ';

UPDATE public."Employees" SET
    "SsnEncrypted" = NULL,
    "TinEncrypted" = NULL,
    "AlienNumberEncrypted" = NULL,
    "PassportNumberEncrypted" = NULL,
    "DriversLicNumEncrypted" = NULL,
    "BirthDate" = NULL,
    "UpdatedAt" = NOW()
WHERE "Notes" = 'OBID:Emp_20250924021859493696_HQ';

UPDATE public."Employees" SET
    "SsnEncrypted" = NULL,
    "TinEncrypted" = NULL,
    "AlienNumberEncrypted" = NULL,
    "PassportNumberEncrypted" = NULL,
    "DriversLicNumEncrypted" = NULL,
    "BirthDate" = NULL,
    "UpdatedAt" = NOW()
WHERE "Notes" = 'OBID:Emp_20260210110823807465_HQ';

UPDATE public."Employees" SET
    "SsnEncrypted" = NULL,
    "TinEncrypted" = NULL,
    "AlienNumberEncrypted" = NULL,
    "PassportNumberEncrypted" = NULL,
    "DriversLicNumEncrypted" = NULL,
    "BirthDate" = NULL,
    "UpdatedAt" = NOW()
WHERE "Notes" = 'OBID:Emp_20250924021732389036_HQ';

UPDATE public."Employees" SET
    "SsnEncrypted" = NULL,
    "TinEncrypted" = NULL,
    "AlienNumberEncrypted" = NULL,
    "PassportNumberEncrypted" = NULL,
    "DriversLicNumEncrypted" = NULL,
    "BirthDate" = NULL,
    "UpdatedAt" = NOW()
WHERE "Notes" = 'OBID:Emp_20260210110947174778_HQ';

UPDATE public."Employees" SET
    "SsnEncrypted" = NULL,
    "TinEncrypted" = NULL,
    "AlienNumberEncrypted" = NULL,
    "PassportNumberEncrypted" = NULL,
    "DriversLicNumEncrypted" = NULL,
    "BirthDate" = NULL,
    "UpdatedAt" = NOW()
WHERE "Notes" = 'OBID:Emp_20250924021753860644_HQ';

UPDATE public."Employees" SET
    "SsnEncrypted" = 'aV1L4eJQi+JvXIii9PmP3leGq7OVO7hN5itz93C1PpFl9svnQRg8',
    "TinEncrypted" = NULL,
    "AlienNumberEncrypted" = NULL,
    "PassportNumberEncrypted" = NULL,
    "DriversLicNumEncrypted" = NULL,
    "BirthDate" = '1961-10-12',
    "UpdatedAt" = NOW()
WHERE "Notes" = 'OBID:Emp_20260210113844695506_HQ';

UPDATE public."Employees" SET
    "SsnEncrypted" = NULL,
    "TinEncrypted" = NULL,
    "AlienNumberEncrypted" = NULL,
    "PassportNumberEncrypted" = NULL,
    "DriversLicNumEncrypted" = NULL,
    "BirthDate" = NULL,
    "UpdatedAt" = NOW()
WHERE "Notes" = 'OBID:Emp_20260210092351547557_HQ';

UPDATE public."Employees" SET
    "SsnEncrypted" = NULL,
    "TinEncrypted" = NULL,
    "AlienNumberEncrypted" = NULL,
    "PassportNumberEncrypted" = NULL,
    "DriversLicNumEncrypted" = NULL,
    "BirthDate" = NULL,
    "UpdatedAt" = NOW()
WHERE "Notes" = 'OBID:Emp_20250924021444358814_HQ';

UPDATE public."Employees" SET
    "SsnEncrypted" = NULL,
    "TinEncrypted" = NULL,
    "AlienNumberEncrypted" = NULL,
    "PassportNumberEncrypted" = NULL,
    "DriversLicNumEncrypted" = NULL,
    "BirthDate" = NULL,
    "UpdatedAt" = NOW()
WHERE "Notes" = 'OBID:Emp_20260210030232761533_HQ';

UPDATE public."Employees" SET
    "SsnEncrypted" = NULL,
    "TinEncrypted" = NULL,
    "AlienNumberEncrypted" = NULL,
    "PassportNumberEncrypted" = NULL,
    "DriversLicNumEncrypted" = NULL,
    "BirthDate" = NULL,
    "UpdatedAt" = NOW()
WHERE "Notes" = 'OBID:Emp_20250924021546173239_HQ';

UPDATE public."Employees" SET
    "SsnEncrypted" = '5JlDlodAc+5G7E/tGjgQvj/twBgNx36o4MPfdApdelqw9CkKEpGF',
    "TinEncrypted" = NULL,
    "AlienNumberEncrypted" = NULL,
    "PassportNumberEncrypted" = NULL,
    "DriversLicNumEncrypted" = NULL,
    "BirthDate" = '1994-07-25',
    "UpdatedAt" = NOW()
WHERE "Notes" = 'OBID:Emp_20240726_01017';

UPDATE public."Employees" SET
    "SsnEncrypted" = 'PtmIAWvX7DSkG3Rh0ii6agpIyrgIDV3S1iSMS9gxWA8MTGK6tg==',
    "TinEncrypted" = NULL,
    "AlienNumberEncrypted" = NULL,
    "PassportNumberEncrypted" = NULL,
    "DriversLicNumEncrypted" = NULL,
    "BirthDate" = '1967-09-24',
    "UpdatedAt" = NOW()
WHERE "Notes" = 'OBID:Emp_20240726_01053';

COMMIT;

-- Summary:
-- Updated: 128
-- BirthDate parsed: 98