#!/usr/bin/env python3
# Compares the KoSIT reports (VARL) of two directories, e.g. the reports of the KoSIT validator on the JVM and
# the reports of KositIkvm: matched scenario, validity, assessment, validation steps and every message
# (id, code, level, xpathLocation, text). Engine, timestamps and document identification are ignored.
#
# usage: compare-reports.py <reports of the JVM validator> <reports of KositIkvm>
import os
import re
import sys
import xml.etree.ElementTree as ET

REP = '{http://www.xoev.de/de/validator/varl/1}'
SCN = '{http://www.xoev.de/de/validator/framework/1/scenarios}'


def summary(path):
    root = ET.parse(path).getroot()
    lines = ['valid=' + root.get('valid', '')]
    name = root.find('.//' + SCN + 'scenario/' + SCN + 'name')
    lines.append('scenario=' + (name.text.strip() if name is not None else 'NONE'))
    assessment = root.find(REP + 'assessment')
    lines.append('assessment=' + (assessment[0].tag.replace(REP, '') if assessment is not None and len(assessment) else ''))
    for step in root.iter(REP + 'validationStepResult'):
        lines.append('step %s valid=%s' % (step.get('id'), step.get('valid')))
        for message in step.findall(REP + 'message'):
            text = re.sub(r'\s+', ' ', ''.join(message.itertext())).strip()
            lines.append('  %s|%s|%s|%s|%s' % (message.get('id'), message.get('code'), message.get('level'), message.get('xpathLocation'), text))
    return lines


def main(expected_dir, actual_dir):
    identical = different = 0
    for name in sorted(os.listdir(expected_dir)):
        actual_path = os.path.join(actual_dir, name)
        if not os.path.exists(actual_path):
            print('missing', name)
            different += 1
            continue
        expected, actual = summary(os.path.join(expected_dir, name)), summary(actual_path)
        if expected == actual:
            identical += 1
            continue
        different += 1
        print('DIFF', name)
        for line in [l for l in expected if l not in actual][:4]:
            print('   jvm  ', line[:250])
        for line in [l for l in actual if l not in expected][:4]:
            print('   ikvm ', line[:250])
    print('%d identical, %d different' % (identical, different))
    return 0 if different == 0 else 1


if __name__ == '__main__':
    sys.exit(main(sys.argv[1], sys.argv[2]))
