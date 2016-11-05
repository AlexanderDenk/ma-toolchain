#! /usr/bin/env python2.7

import sys
import PyBool_public_interface as Bool

expression = Bool.parse_std(expressionFileInput)
expression = expression["main_expr"]
expression = Bool.simplify(expression)
expression = Bool.exp_cnf(expression)
result = Bool.print_expr(expression)
output = open(expressionFileOutput, 'w')
output.write(result)
output.close()
