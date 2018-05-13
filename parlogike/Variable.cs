
namespace Parlogike_
{
    public class Variable
    {
        public bool isNumber = false;
        public string strVal = "";
        public float fVal = 0;

        public Variable(string value)
        {
            strVal = value;
            isNumber = false;
        }

        public Variable(float value)
        {
            fVal = value;
            isNumber = true;
        }

        public Variable()
        {
        }
        public static Variable operator ++(Variable b)
        {
            b.fVal++;
            return b;
        }
        public static Variable operator --(Variable b)
        {
            b.fVal--;
            return b;
        }
        /*public static Variable operator += (Variable a , float b)
        {
            a.fVal += b;
            return a;
        }
        public static Variable operator -= (Variable a , float b)
        {
            a.fVal -= b;
            return a;
        }
        public static Variable operator = ( string value)
        {
            a.strVal = value;
            a.isNumber = false;
            return a;
        }

        Variable operator = (float value)
        {
            fVal = value;
            isNumber = true;
            return *this;
        }

        Variable operator = (Variable b)
        {
            isNumber = b.isNumber;
            fVal = b.fVal;
            strVal = b.strVal;
        }

        Variable operator +(float value)
        {
            if (isNumber)
                return Variable(fVal + value);
        }

        bool operator ==(string value)
        {
            if (!isNumber && !value.compare(strVal))
                return true;
            return false;
        }

        bool operator ==(float value)
        {
            if (isNumber && abs(value - fVal) <= 0.05)
                return true;
            return false;
        }*/
        public string toString(bool mutate = false)
        {
            if (!isNumber)
                return strVal;
            /*ostringstream ss;
            ss << fVal;*/
            string ret = fVal.ToString();
            if (mutate)
            {
                isNumber = false;
                strVal = ret;
            }
            return ret;
        }
        public float toNumber(bool mutate = false)
        {
            if (isNumber)
                return fVal;

            float ret = float.Parse(strVal);//atof(strVal.c_str());
            if (mutate)
            {
                isNumber = true;
                fVal = ret;
            }
            return ret;
        }
    };
};